using System;
using System.Threading.Tasks;
using BinanceExchange.API.Enums;
using BinanceExchange.API.Models.WebSocket;

namespace BinanceExchange.API.Websockets
{
    public interface IBinanceWebSocketClient
    {
        /// <summary>
        /// Connect to the Kline WebSocket
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="interval"></param>
        /// <param name="messageEventHandler"></param>
        /// <returns></returns>
        Task<Guid> ConnectToKlineWebSocketAsync(string symbol, KlineInterval interval, BinanceWebSocketMessageHandler<BinanceKlineData> messageEventHandler);

        /// <summary>
        /// Connect to the Depth WebSocket
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="messageEventHandler"></param>
        /// <returns></returns>
        Task<Guid> ConnectToDepthWebSocketAsync(string symbol, BinanceWebSocketMessageHandler<BinanceDepthData> messageEventHandler);

        /// <summary>
        /// Connect to the Trades WebSocket
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="messageEventHandler"></param>
        /// <returns></returns>
        Task<Guid> ConnectToTradesWebSocketAsync(string symbol, BinanceWebSocketMessageHandler<BinanceAggregateTradeData> messageEventHandler);

        /// <summary>
        /// Connect to the UserData WebSocket
        /// </summary>
        /// <param name="userDataMessageHandlers"></param>
        /// <returns></returns>
        Task<Guid> ConnectToUserDataWebSocket(UserDataWebSocketMessages userDataMessageHandlers);

        /// <summary>
        /// Close a specific WebSocket instance using the Guid provided on creation
        /// </summary>
        /// <param name="id"></param>
        /// 
        void CloseWebSocketInstance(Guid id);
    }
}