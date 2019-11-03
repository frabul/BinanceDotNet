using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using BinanceExchange.API.Client.Interfaces;
using BinanceExchange.API.Enums;
using BinanceExchange.API.Extensions;
using BinanceExchange.API.Models.WebSocket;
using BinanceExchange.API.Utility;
using Newtonsoft.Json; 
using IWebSocketResponse = BinanceExchange.API.Models.WebSocket.Interfaces.IWebSocketResponse;

namespace BinanceExchange.API.Websockets
{
    /// <summary>
    /// Abstract class for creating WebSocketClients 
    /// </summary>
    public class BinanceWebSocketClient
    {
        protected SslProtocols SupportedProtocols { get; } = SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls;

        /// <summary> 
        /// Base WebSocket URI for Binance API
        /// </summary>
        protected string BaseWebsocketUri = "wss://stream.binance.com:9443/ws";

        /// <summary>
        /// Combined WebSocket URI for Binance API
        /// </summary>
        protected string CombinedWebsocketUri = "wss://stream.binance.com:9443/stream?streams";

        /// <summary>
        /// Used for deletion on the fly
        /// </summary>
        protected Dictionary<Guid, BinanceWebSocket> ActiveWebSockets; 
        protected readonly IBinanceClient BinanceClient;
        protected NLog.Logger Logger;

        protected const string AccountEventType = "outboundAccountInfo";
        protected const string OutboundAccountPosition = "outboundAccountPosition";
        protected const string OrderTradeEventType = "executionReport";
        protected const string ListStatus = "listStatus";

        public BinanceWebSocketClient(IBinanceClient binanceClient, NLog.Logger logger = null)
        {
            BinanceClient = binanceClient;
            ActiveWebSockets = new Dictionary<Guid, BinanceWebSocket>(); 
            Logger = logger ?? NLog.LogManager.GetCurrentClassLogger();
        }


        /// <summary>
        /// Connect to the Kline WebSocket
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="interval"></param>
        /// <param name="messageEventHandler"></param>
        /// <returns></returns>
        public async Task<Guid> ConnectToKlineWebSocketAsync(string symbol, KlineInterval interval, BinanceWebSocketMessageHandler<BinanceKlineData> messageEventHandler)
        {
            Guard.AgainstNullOrEmpty(symbol, nameof(symbol));
            Logger.Debug("Connecting to Kline Web Socket");
            var endpoint = new Uri($"{BaseWebsocketUri}/{symbol.ToLower()}@kline_{EnumExtensions.GetEnumMemberValue(interval)}");
            return await CreateBinanceWebSocketAsync(endpoint, messageEventHandler);
        }

        /// <summary>
        /// Connect to the Depth WebSocket
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="messageEventHandler"></param>
        /// <returns></returns>
        public async Task<Guid> ConnectToDepthWebSocketAsync(string symbol, BinanceWebSocketMessageHandler<BinanceDepthData> messageEventHandler)
        {
            Guard.AgainstNullOrEmpty(symbol, nameof(symbol));
            Logger.Debug("Connecting to Depth Web Socket");
            var endpoint = new Uri($"{BaseWebsocketUri}/{symbol.ToLower()}@depth");
            return await CreateBinanceWebSocketAsync(endpoint, messageEventHandler);
        }

        /// <summary>
        /// Connect to thePartial Book Depth Streams
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="messageEventHandler"></param>
        /// <returns></returns>
        public async Task<Guid> ConnectToPartialDepthWebSocketAsync(string symbol, PartialDepthLevels levels, BinanceWebSocketMessageHandler<BinancePartialData> messageEventHandler)
        {
            Guard.AgainstNullOrEmpty(symbol, nameof(symbol));
            Logger.Debug("Connecting to Partial Depth Web Socket");
            var endpoint = new Uri($"{BaseWebsocketUri}/{symbol.ToLower()}@depth{(int)levels}");
            return await CreateBinanceWebSocketAsync(endpoint, messageEventHandler);
        }

        /// <summary>
        /// Connect to the Combined Depth WebSocket
        /// </summary>
        /// <param name="symbols"></param>
        /// <param name="messageEventHandler"></param>
        /// <returns></returns>
        public async Task<Guid> ConnectToDepthWebSocketCombinedAsync(string symbols, BinanceWebSocketMessageHandler<BinanceCombinedDepthData> messageEventHandler)
        {
            Guard.AgainstNullOrEmpty(symbols, nameof(symbols));
            symbols = PrepareCombinedSymbols.CombinedDepth(symbols);
            Logger.Debug("Connecting to Combined Depth Web Socket");
            var endpoint = new Uri($"{CombinedWebsocketUri}={symbols}");
            return await CreateBinanceWebSocketAsync(endpoint, messageEventHandler);
        }
        /// <summary>
        /// Connect to the Combined Partial Depth WebSocket
        /// </summary>
        /// <param name="symbols"></param>
        /// <param name="depth"></param>
        /// <param name="messageEventHandler"></param>
        /// <returns></returns>
        public async Task<Guid> ConnectToPartialDepthWebSocketCombinedAsync(string symbols, string depth, BinanceWebSocketMessageHandler<BinancePartialDepthData> messageEventHandler)
        {
            Guard.AgainstNullOrEmpty(symbols, nameof(symbols));
            Guard.AgainstNullOrEmpty(depth, nameof(depth));
            symbols = PrepareCombinedSymbols.CombinedPartialDepth(symbols, depth);
            Logger.Debug("Connecting to Combined Partial Depth Web Socket");
            var endpoint = new Uri($"{CombinedWebsocketUri}={symbols}");
            return await CreateBinanceWebSocketAsync(endpoint, messageEventHandler);
        }

        /// <summary>
        /// Connect to the Trades WebSocket
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="messageEventHandler"></param>
        /// <returns></returns>https://github.com/glitch100/BinanceDotNet/issues
        public async Task<Guid> ConnectToTradesWebSocketAsync(string symbol, BinanceWebSocketMessageHandler<BinanceAggregateTradeData> messageEventHandler)
        {
            Guard.AgainstNullOrEmpty(symbol, nameof(symbol));
            Logger.Debug("Connecting to Trades Web Socket");
            var endpoint = new Uri($"{BaseWebsocketUri}/{symbol.ToLower()}@aggTrade");
            return await CreateBinanceWebSocketAsync(endpoint, messageEventHandler);
        }

        /// <summary>
        /// Connect to the UserData WebSocket
        /// </summary>
        /// <param name="userDataMessageHandlers"></param>
        /// <returns></returns>
        public async Task<Guid> ConnectToUserDataWebSocket(UserDataWebSocketMessages userDataMessageHandlers)
        {
            Guard.AgainstNull(BinanceClient, nameof(BinanceClient));
            Logger.Debug("Connecting to User Data Web Socket");
            var streamResponse = await BinanceClient.StartUserDataStream();

            var endpoint = new Uri($"{BaseWebsocketUri}/{streamResponse.ListenKey}");
            return await CreateUserDataBinanceWebSocketAsync(endpoint, streamResponse.ListenKey, userDataMessageHandlers);
        }

        private async Task<Guid> CreateUserDataBinanceWebSocketAsync(Uri endpoint, string listenKey, UserDataWebSocketMessages userDataWebSocketMessages)
        {

            var websocket = new BinanceWebSocket(  listenKey);

            Action<WebSocketWrapper, string> onMsg = (sender, msg) =>
            {
                Logger.Trace($"WebSocket Message Received on Endpoint: {endpoint.AbsoluteUri}");
                var primitive = JsonConvert.DeserializeObject<BinanceWebSocketResponse>(msg);
                switch (primitive.EventType)
                {
                    case AccountEventType:
                        var userData = JsonConvert.DeserializeObject<BinanceAccountUpdateData>(msg);
                        userDataWebSocketMessages.AccountUpdateMessageHandler(userData);
                        break;
                    case OrderTradeEventType:
                        var orderTradeData = JsonConvert.DeserializeObject<BinanceTradeOrderData>(msg);
                        if (orderTradeData.ExecutionType == ExecutionType.Trade)
                        {
                            userDataWebSocketMessages.TradeUpdateMessageHandler(orderTradeData);
                        }
                        else
                        {
                            userDataWebSocketMessages.OrderUpdateMessageHandler(orderTradeData);
                        }
                        break;
                    case OutboundAccountPosition:
                        //todo
                        break;
                    case ListStatus:
                        //todo
                        break;
                    default:
                        Logger.Error("Unknown EventType for user data stream");
                        throw new ArgumentOutOfRangeException("Unknown EventType for user data stream");
                }
            };
             
            if (!ActiveWebSockets.ContainsKey(websocket.Id))
            {
                ActiveWebSockets.Add(websocket.Id, websocket);
            }
            await websocket.ConnectAsync(endpoint, onMsg); 
           
            return websocket.Id;
        }

        private async Task<Guid> CreateBinanceWebSocketAsync<T>(Uri endpoint, BinanceWebSocketMessageHandler<T> messageEventHandler) where T : IWebSocketResponse
        {
            var websocket = new BinanceWebSocket(); 

            Action<WebSocketWrapper, string> onRecv = (sender, msg) =>
            {
                Logger.Debug($"WebSocket Messge Received on: {endpoint.AbsoluteUri}");
                //TODO: Log message received
                var data = JsonConvert.DeserializeObject<T>(msg);
                messageEventHandler(data);
            };

            await websocket.ConnectAsync(endpoint, onRecv); 

            if (!ActiveWebSockets.ContainsKey(websocket.Id))
            {
                ActiveWebSockets.Add(websocket.Id, websocket);
            }
            
            return websocket.Id;
        }

        /// <summary>
        /// Close a specific WebSocket instance using the Guid provided on creation
        /// If it is a user data stream socket then also the listenKey is closed
        /// </summary>
        /// <param name="id"></param>
        /// <param name="fromError"></param>
        public void CloseWebSocketInstance(Guid id, bool fromError = false)
        {
            if (ActiveWebSockets.ContainsKey(id))
            {
                var ws = ActiveWebSockets[id];
                ActiveWebSockets.Remove(id);
                if (!fromError)
                {
                    try { _ = ws.CloseAsync(); }
                    catch { }
                }
                if (ws.ListenKey != null)
                {
                    this.BinanceClient.CloseUserDataStream(ws.ListenKey).Start();
                }
            }
            else
            {
                throw new Exception($"No Websocket exists with the Id {id.ToString()}");
            }
        }

        /// <summary>
        /// Checks whether a specific WebSocket instance is active or not using the Guid provided on creation
        /// </summary>
        public bool IsAlive(Guid id)
        {
            if (ActiveWebSockets.ContainsKey(id))
            {
                var ws = ActiveWebSockets[id];
                return ws.IsAlive;
            }
            else
            {
                throw new Exception($"No Websocket exists with the Id {id.ToString()}");
            }
        }

        /// <summary>
        /// Send a ping to the websocket instance
        /// </summary>
        /// <param name="guid">Websocket instance GUID</param>
        /// <returns>True if the socket server replied, false otherwise</returns>
        public bool PingWebSocketInstance(Guid guid)
        {
            if (ActiveWebSockets.ContainsKey(guid))
            {
                var ws = ActiveWebSockets[guid];
                //todo implement ping 
                return ws.IsAlive;
            }
            else
            {
                throw new Exception($"No Websocket exists with the Id {guid.ToString()}");
            }
        }
    }
}
