using System;
using WebSocketSharp;

namespace BinanceExchange.API.Websockets
{
    /// <summary>
    /// Built around WebSocket for future improvements. For all intents and purposes, this is the same as the WebSocket
    /// </summary>
    public class BinanceWebSocket : WebSocket
    {
        public Guid Id { get; private set; }
        public string ListenKey { get; private set; }
        public BinanceWebSocket(string url) : base(url)
        {
            Id = Guid.NewGuid();
        }

        public BinanceWebSocket(string url, string listenKey) : this(url)
        {
            ListenKey = listenKey;
        }
        public BinanceWebSocket(string url, string listenKey, params string[] protocols) : base(url, protocols)
        {
            Id = Guid.NewGuid();
            ListenKey = listenKey;
        }
    }
}