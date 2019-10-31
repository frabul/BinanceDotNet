using System;

namespace BinanceExchange.API.Websockets
{
    /// <summary>
    /// Built around WebSocket for future improvements. For all intents and purposes, this is the same as the WebSocket
    /// </summary>
    public class BinanceWebSocket : WebSocketWrapper
    { 
        public Guid Id { get; private set; }
        public string ListenKey { get; private set; }
        

        public BinanceWebSocket( )
        { 
            Id = Guid.NewGuid();
        }

        public BinanceWebSocket(  string listenKey)
        {
            Id = Guid.NewGuid();
            ListenKey = listenKey;
        }
 
    }
}