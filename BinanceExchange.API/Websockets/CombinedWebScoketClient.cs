using BinanceExchange.API.Enums;
using BinanceExchange.API.Extensions;
using BinanceExchange.API.Models.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Security.Authentication;
using System.Text; 
using System.Timers;

namespace BinanceExchange.API.Websockets
{
    public class CombinedWebSocketClient
    {
        private int StreamsPerSocket = 20;
        private string CombinedWebsocketUri = "wss://stream.binance.com:9443/stream?streams=";
        private SslProtocols SupportedProtocols = SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls;
        private Dictionary<string, SockStream> Streams = new Dictionary<string, SockStream>();

        private List<CombinedWebSocket> ActiveWebSockets = new List<CombinedWebSocket>();
        private Timer RefreshTimer;
        private DateTime NextRebuildTime = DateTime.MinValue;

        public CombinedWebSocketClient()
        {
            RefreshTimer = new Timer()
            {
                Interval = 1000,
                AutoReset = false,
                Enabled = true,

            };
            RefreshTimer.Elapsed += RefreshTimer_Elapsed;
            RefreshTimer.Start();
        }

        private void RefreshTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (DateTime.Now > NextRebuildTime)
            {
                NextRebuildTime = NextRebuildTime.AddSeconds(30);
                try { RebuildSocketsAsync(); }
                catch { }
            }
            RefreshTimer.Start();
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
            var sockName = $"{symbol.ToLower()}@depth{(int)levels}";
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
                    NextRebuildTime = DateTime.Now.AddSeconds(2);
                }
            }
            if (stream is Stream<T> tstream)
                tstream.Subscribe(handler);
            else
                throw new Exception("Wrong handler for the stream.");

        }

        private async System.Threading.Tasks.Task RebuildSocketsAsync()
        {
            //close sockets older than 6h or that are not responsive
            CombinedWebSocket[] socks;
            lock (ActiveWebSockets)
                socks = ActiveWebSockets.ToArray();
            foreach (var sock in socks)
            {
                bool needClose = !(sock.WatchDog.Elapsed > TimeSpan.FromSeconds(70) || DateTime.Now > sock.CreationTime.AddHours(2));
                if (DateTime.Now > sock.LastPing.AddMinutes(20))
                    needClose &= sock.Ping();
                if (needClose)
                    CloseSocket(sock);
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

        private async System.Threading.Tasks.Task OpenWebSocketAsync(SockStream[] streamsPerSocket)
        {
            string endpoint = this.CombinedWebsocketUri;
            foreach (var str in streamsPerSocket)
            {
                endpoint += str.SockName + "/";
            }
            endpoint = endpoint.Remove(endpoint.Length - 1);
            var websocket = new CombinedWebSocket( );
            websocket.Streams = streamsPerSocket;
          
            //websocket.Log.Level = (LogLevel)(LogLevel.Fatal + 1);
            //websocket.OnOpen += (sender, e) =>
            //{
            //    websocket.WatchDog.Restart();
            //};
            Action<WebSocketWrapper, string> onMsg = (sender, msg) =>
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
            };
            await websocket.ConnectAsync(endpoint, onMsg).ContinueWith(firstTask => websocket.WatchDog.Restart());
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
                    try {   sock.CloseAsync(); }
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
