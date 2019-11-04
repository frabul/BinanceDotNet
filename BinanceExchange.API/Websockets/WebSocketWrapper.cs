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
        private readonly ClientWebSocket Socket;
        private readonly CancellationTokenSource CancelToken = new CancellationTokenSource();
        private Action<WebSocketWrapper, string> OnMessage;
        private readonly byte[] rawBuffer = new byte[ReceiveChunkSize];

        public bool IsDisocnnected { get; private set; } = true;
        public bool IsAlive => !IsDisocnnected;

        public WebSocketWrapper()
        {
            Socket = new ClientWebSocket();
            Socket.Options.KeepAliveInterval = TimeSpan.FromSeconds(20); 
        }

        public async Task ConnectAsync(string url, Action<WebSocketWrapper, string> onMessage)
        {
            await ConnectAsync(new Uri(url), onMessage);
        }

        public async Task ConnectAsync(Uri uri, Action<WebSocketWrapper, string> onMessage)
        {
            await Socket.ConnectAsync(uri, CancelToken.Token);
            OnMessage = onMessage;
            IsDisocnnected = false;
            //start reading messages
            _ = ReadMessages();
        }

      
        public async Task ReadMessages()
        {
            try
            {
                while (Socket.State == WebSocketState.Open)
                {
                    var stringResult = new StringBuilder();
                    WebSocketReceiveResult result;

                    do
                    {
                        result = await Socket.ReceiveAsync(new ArraySegment<byte>(rawBuffer), CancelToken.Token);

                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            await Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                            CallOnDisconnected();
                        }
                        else
                        {
                            var str = Encoding.UTF8.GetString(rawBuffer, 0, result.Count);
                            stringResult.Append(str);
                        }

                    } while (!result.EndOfMessage);

                    OnMessage(this, stringResult.ToString());
                    stringResult.Clear();

                }
            }
            catch (Exception)
            {
                CallOnDisconnected();
            }
            finally
            {
                Socket.Dispose();
            }

        }

        private void CallOnDisconnected()
        {
            IsDisocnnected = true;
        }

        public async Task CloseAsync()
        {
            try
            {
                if (!IsDisocnnected)
                {
                    await Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancelToken.Token);
                    Socket.Dispose();
                }
            }
            catch
            {

            }
            finally
            {
                CancelToken.Dispose();
            }
        }

    }
}
