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
        System.Net.WebSockets.ClientWebSocket Socket;
        CancellationTokenSource CancelToken = new CancellationTokenSource();
        Action<WebSocketWrapper, string> OnMessage;
        byte[] rawBuffer = new byte[100000];
        public bool Disconnected { get; private set; } = true;
        public bool IsAlive { get; internal set; } => !Disconnected;

        public async Task ConnectAsync(string url, Action<WebSocketWrapper, string> onMessage)
        {
            await ConnectAsync( new Uri(url), onMessage);
        }
        public async Task ConnectAsync(Uri uri, Action<WebSocketWrapper, string> onMessage)
        { 
            await Socket.ConnectAsync(uri, CancelToken.Token);
            OnMessage = onMessage;
            Disconnected = false;
            //start reading messages
            ReadMessages();
        }
     
        public bool Ping()
        {
            return Disconnected;
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
            Disconnected = true;
        }

        public async Task CloseAsync()
        {
            try
            {
                if (!Disconnected)
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
