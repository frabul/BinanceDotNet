using BinanceExchange.API.Client.Interfaces; 

namespace BinanceExchange.API.Websockets
{
    /// <summary>
    /// Used for manual management of WebSockets per instance. Only use this if you want to manually manage, open, and close your websockets.
    /// </summary>
    public class InstanceBinanceWebSocketClient : BinanceWebSocketClient, IBinanceWebSocketClient
    {
        public InstanceBinanceWebSocketClient(IBinanceClient binanceClient, NLog.Logger logger = null) :
            base(binanceClient, logger)
        {
        }
    }
}