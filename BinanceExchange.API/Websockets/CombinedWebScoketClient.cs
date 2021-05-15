using BinanceExchange.API.Enums;
using BinanceExchange.API.Extensions;
using BinanceExchange.API.Models.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace BinanceExchange.API.Websockets
{
    public class CombinedWebSocketClient
    {
        private readonly int StreamsPerSocket = 50;
        private readonly string CombinedWebsocketUri = "wss://stream.binance.com:9443/stream?streams=";
        private readonly Logger Logger = LogManager.GetLogger("CombinedWebSocketClient");
        private readonly Dictionary<string, SockStream> Streams = new Dictionary<string, SockStream>();
        private readonly List<CombinedWebSocket> ActiveWebSockets = new List<CombinedWebSocket>();
        private DateTime NextRebuildTime = DateTime.MinValue;


        public CombinedWebSocketClient()
        {
            _ = WebSocketsRefresher();
        }

        private async Task WebSocketsRefresher()
        {
            while (true)
            {
                if (DateTime.Now > NextRebuildTime)
                {
                    NextRebuildTime = NextRebuildTime.AddSeconds(30);
                    try { _ = RebuildSocketsAsync(); }
                    catch { }
                }
                await Task.Delay(1000);
            }
        }

        public void Unsubscribe<T>(Action<T> action)
        {
            lock (Streams)
            {
                foreach (var str in Streams.Values)
                    str.Unsubscribe(action);
            }
        }

        public void SubscribeKlineStream(string symbol, KlineInterval interval, Action<BinanceKlineData> messageEventHandler)
        {
            var sockName = $"{symbol.ToLower()}@kline_{EnumExtensions.GetEnumMemberValue(interval)}";
            AddStreamSubscriber(sockName, messageEventHandler);
        }

        public void SubscribePartialDepthStream(string symbol, PartialDepthLevels levels, Action<BinancePartialData> messageEventHandler)
        {
            var sockName = $"{symbol.ToLower()}@depth{(int)levels}@1000ms";
            AddStreamSubscriber(sockName, messageEventHandler);
        }

        private void AddStreamSubscriber<T>(string sockName, Action<T> handler)
        {
            SockStream stream;
            lock (Streams)
            {
                if (!Streams.TryGetValue(sockName, out stream))
                {
                    stream = new Stream<T>(sockName);
                    Streams.Add(stream.SockName, stream);
                    NextRebuildTime = DateTime.Now.AddSeconds(5);
                }
            }
            if (stream is Stream<T> tstream)
                tstream.Subscribe(handler);
            else
                throw new Exception("Wrong handler for the stream.");

        }

        private async Task RebuildSocketsAsync()
        {
            //close sockets older than 6h or that are not responsive
            CombinedWebSocket[] socks;
            lock (ActiveWebSockets)
                socks = ActiveWebSockets.ToArray();
            foreach (var sock in socks)
            {
                bool sockWatchDogFail = sock.WatchDog.Elapsed > TimeSpan.FromSeconds(30);
                bool sockIsOld = DateTime.Now > sock.CreationTime.AddHours(2);

                if (sockWatchDogFail || sockIsOld || !sock.IsAlive)
                {
                    Logger.Log(
                        sockIsOld ? LogLevel.Trace : LogLevel.Debug,
                        $"A combined websocket needs to be closed: sockWatchDogFail={sockWatchDogFail}, sockIsOld={sockIsOld}, Disconnected={sock.IsDisocnnected}  ");

                    CloseSocket(sock);
                }
            }

            //check that all streams have a socket
            Queue<SockStream> streamsWithNoSocket;
            lock (ActiveWebSockets)
            {
                lock (Streams)
                    streamsWithNoSocket = new Queue<SockStream>(Streams.Values.Where(st => !ActiveWebSockets.Any(sock => sock.Streams.Any(ss => ss == st))).ToArray());
            }

            while (streamsWithNoSocket.Count > 0)
            {
                CombinedWebSocket sockToRebuild;
                lock (ActiveWebSockets)
                    sockToRebuild = ActiveWebSockets.FirstOrDefault(s => s.Streams.Length < StreamsPerSocket);

                List<SockStream> streamsForSocket = new List<SockStream>();
                if (sockToRebuild != null)
                {
                    CloseSocket(sockToRebuild);
                    streamsForSocket.AddRange(sockToRebuild.Streams);
                }

                while (streamsForSocket.Count < this.StreamsPerSocket && streamsWithNoSocket.Count > 0)
                    streamsForSocket.Add(streamsWithNoSocket.Dequeue());

                try
                {
                    await OpenWebSocketAsync(streamsForSocket.ToArray());
                }
                catch
                {
                    //put all streams that were scheduled for this socket again in streamsWithNoSocket queue
                    foreach (var sock in streamsForSocket)
                        streamsWithNoSocket.Enqueue(sock);
                }

            }

        }

        private async Task OpenWebSocketAsync(SockStream[] streamsPerSocket)
        {
            string endpoint = this.CombinedWebsocketUri;
            foreach (var str in streamsPerSocket)
            {
                endpoint += str.SockName + "/";
            }
            endpoint = endpoint.Remove(endpoint.Length - 1);
            var websocket = new CombinedWebSocket { Streams = streamsPerSocket };
            void onMsg(WebSocketWrapper sender, string msg)
            {
                try
                {
                    var datum = JsonConvert.DeserializeObject<BinanceCombinedWebsocketData>(msg);
                    SockStream stream;
                    bool found;
                    lock (Streams)
                        found = Streams.TryGetValue(datum.StreamName, out stream);
                    if (found)
                        stream.Pulse(datum.RawData);
                    else
                        Console.WriteLine("sockNotFound");

                    websocket.WatchDog.Restart();
                }
                catch { }
            }

            await websocket.ConnectAsync(endpoint, onMsg);
            websocket.WatchDog.Restart();
            lock (ActiveWebSockets)
                ActiveWebSockets.Add(websocket);
        }

        private void CloseSocket(CombinedWebSocket sock)
        {
            lock (ActiveWebSockets)
            {
                if (ActiveWebSockets.Contains(sock))
                {
                    ActiveWebSockets.Remove(sock);
                    try { _ = sock.CloseAsync(); }
                    catch { }
                }
            }
        }

        abstract class SockStream
        {
            protected SockStream(string sockName)
            {
                SockName = sockName;
            }

            public string SockName { get; private set; }

            abstract public void Pulse(JObject jsonData);
            abstract public void Unsubscribe(Delegate del);
        }

        class Stream<T> : SockStream
        {
            public List<Action<T>> Subscribes { get; private set; } = new List<Action<T>>();

            public Stream(string sockName) : base(sockName)
            {
            }


            public void Subscribe(Action<T> action)
            {
                lock (Subscribes)
                    if (!Subscribes.Contains(action))
                        Subscribes.Add(action);
                    else
                    { }
            }

            public override void Unsubscribe(Delegate action)
            {
                lock (Subscribes)
                    if (action is Action<T> && Subscribes.Contains(action))
                        Subscribes.Remove((Action<T>)action);
            }

            public override void Pulse(JObject jsonData)
            {
                var resp = jsonData.ToObject<T>(); //  Newtonsoft.Json.JsonConvert.DeserializeObject<T>(jsonData);
                lock (Subscribes)
                    foreach (var sub in Subscribes)
                        sub.Invoke(resp);
            }

        }



        class CombinedWebSocket : WebSocketWrapper
        {

            public Stopwatch WatchDog { get; } = new Stopwatch();
            public SockStream[] Streams { get; internal set; }
            public DateTime CreationTime { get; } = DateTime.Now;
            public DateTime LastPing;
            public CombinedWebSocket()
            {
                LastPing = DateTime.Now;
            }
        }
    }


}
