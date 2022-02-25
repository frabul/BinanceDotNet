using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BinanceExchange.API.Websockets
{
    public class WebSocketWrapper
    {
        private const int ReceiveChunkSize = 10000;
        private const int SendChunkSize = 1024;
        private ClientWebSocket Socket;
        private CancellationTokenSource CancelToken;
        private Action<WebSocketWrapper, string> OnMessage;
        private readonly byte[] rawBuffer = new byte[ReceiveChunkSize];
        private Task WorkerTask;
        private volatile bool DisconnectRequested = false;
        public bool IsDisocnnected { get; private set; } = true;
        public bool IsAlive => !IsDisocnnected;

        public WebSocketWrapper()
        {

        }

        public async Task ConnectAsync(string url, Action<WebSocketWrapper, string> onMessage)
        {
            await ConnectAsync(new Uri(url), onMessage);
        }

        public async Task ConnectAsync(Uri uri, Action<WebSocketWrapper, string> onMessage)
        {
            if (IsDisocnnected)
            {
                Socket = new ClientWebSocket();
                Socket.Options.KeepAliveInterval = TimeSpan.FromSeconds(20);
                CancelToken = new CancellationTokenSource();
                await Socket.ConnectAsync(uri, CancelToken.Token);
                OnMessage = onMessage;
                IsDisocnnected = false;
                //start reading messages
                WorkerTask = Task.Run(ReadMessages);
            }
            else
            {
                throw new InvalidOperationException("Socket is already connected");
            }
        }



        public async Task ReadMessages()
        {
            try
            {
                while (Socket.State == WebSocketState.Open && !DisconnectRequested)
                {
                    var stringResult = new StringBuilder();
                    WebSocketReceiveResult result;
                    do
                    {
                        result = await Socket.ReceiveAsync(new ArraySegment<byte>(rawBuffer), CancelToken.Token);

                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            await DisconnectAndDispose();
                        }
                        else
                        {
                            var str = Encoding.UTF8.GetString(rawBuffer, 0, result.Count);
                            stringResult.Append(str);
                        }
                        if (!result.EndOfMessage)
                            await Task.Delay(5);
                    } while (!result.EndOfMessage);

                    OnMessage(this, stringResult.ToString());
                    stringResult.Clear();
                }
            }
            catch (Exception)
            {

            }
            finally
            {
                await CloseAsync();
            }

        }

        private async Task DisconnectAndDispose()
        {
            try
            {
                if (!IsDisocnnected)
                    await Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancelToken.Token);
            }
            catch
            {

            }
            finally
            {
                try { Socket.Dispose(); } catch { }
                try { CancelToken.Dispose(); } catch { }
                IsDisocnnected = true;
            }
        }
        public async Task CloseAsync()
        {
            DisconnectRequested = true;
            await WorkerTask;
            DisconnectRequested = false;
        }

    }
}
