using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using BinanceExchange.API.Converter;
using BinanceExchange.API.Enums;
using BinanceExchange.API.Models.Request;
using BinanceExchange.API.Models.Request.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace BinanceExchange.API
{
    public static class Endpoints
    {

        private static readonly JsonSerializerSettings _settings = new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore,
            FloatParseHandling = FloatParseHandling.Decimal
        };
        private static readonly JsonSerializer DefaultSerializer = JsonSerializer.Create(_settings);
        internal static string APIBaseUrl2 = "https://api1.binance.com";

        /// <summary>
        /// Defaults to API binance domain (https)
        /// </summary>

        /// <summary>
        /// Defaults to WAPI binance domain (https)
        /// </summary>
        internal static string WAPIBaseUrl = "https://api1.binance.com/wapi";

        private static string APIPrefix { get; } = "https://api1.binance.com/api";
        private static string WAPIPrefix { get; } = $"{WAPIBaseUrl}";

        public static class UserStream
        {
            internal static string ApiVersion = "v1";

            /// <summary>
            /// Start a user data stream
            /// </summary>
            public static BinanceEndpointData StartUserDataStream => new BinanceEndpointData(new Uri($"{APIPrefix}/{ApiVersion}/userDataStream"), EndpointSecurityType.ApiKey);

            /// <summary>
            /// Ping a user data stream to prevent a timeout
            /// </summary>
            public static BinanceEndpointData KeepAliveUserDataStream(string listenKey)
            {
                return new BinanceEndpointData(new Uri($"{APIPrefix}/{ApiVersion}/userDataStream?listenKey={listenKey}"),
                    EndpointSecurityType.ApiKey);
            }

            /// <summary>
            /// Close a user data stream to prevent
            /// </summary>
            public static BinanceEndpointData CloseUserDataStream(string listenKey)
            {
                return new BinanceEndpointData(new Uri($"{APIPrefix}/{ApiVersion}/userDataStream?listenKey={listenKey}"),
                    EndpointSecurityType.ApiKey);
            }
        }

        public static class General
        {
            internal static string ApiVersion = "v1";

            /// <summary>
            /// Test connectivity to the Rest API.
            /// </summary>
            public static BinanceEndpointData TestConnectivity => new BinanceEndpointData(new Uri($"{APIPrefix}/{ApiVersion}/ping"), EndpointSecurityType.None);

            /// <summary>
            /// Test connectivity to the Rest API and get the current server time.
            /// </summary>
            public static BinanceEndpointData ServerTime => new BinanceEndpointData(new Uri($"{APIPrefix}/{ApiVersion}/time"), EndpointSecurityType.None);

            /// <summary>
            /// Current exchange trading rules and symbol information.
            /// </summary>
            public static BinanceEndpointData ExchangeInfo => new BinanceEndpointData(new Uri($"{APIPrefix}/{ApiVersion}/exchangeInfo"), EndpointSecurityType.None);

        }

        public static class MarketData
        {
            internal static string ApiVersion = "v1";

            /// <summary>
            /// Gets the order book with all bids and asks
            /// </summary>
            public static BinanceEndpointData OrderBook(string symbol, int limit, bool useCache = false)
            {
                return new BinanceEndpointData(new Uri($"{APIPrefix}/{ApiVersion}/depth?symbol={symbol}&limit={limit}"), EndpointSecurityType.None, useCache);
            }

            /// <summary>
            /// Get compressed, aggregate trades. Trades that fill at the time, from the same order, with the same price will have the quantity aggregated.
            /// </summary>
            public static BinanceEndpointData CompressedAggregateTrades(GetCompressedAggregateTradesRequest request)
            {
                var queryString = GenerateQueryStringFromData(request);
                return new BinanceEndpointData(new Uri($"{APIPrefix}/{ApiVersion}/aggTrades?{queryString}"), EndpointSecurityType.None);
            }

            /// <summary>
            /// Kline/candlestick bars for a symbol. Klines are uniquely identified by their open time.
            /// </summary>
            public static BinanceEndpointData KlineCandlesticks(GetKlinesCandlesticksRequest request)
            {
                var queryString = GenerateQueryStringFromData(request);
                return new BinanceEndpointData(new Uri($"{APIPrefix}/{ApiVersion}/klines?{queryString}"), EndpointSecurityType.None);
            }

            /// <summary>
            /// 24 hour price change statistics.
            /// </summary>
            public static BinanceEndpointData DayPriceTicker(string symbol)
            {
                return new BinanceEndpointData(new Uri($"{APIPrefix}/{ApiVersion}/ticker/24hr?symbol={symbol}"),
                    EndpointSecurityType.None);
            }
             
            /// <summary>
            /// Latest price for all symbols.
            /// </summary>
            public static BinanceEndpointData AllSymbolsPriceTicker => new BinanceEndpointData(new Uri($"{APIPrefix}/v3/ticker/price"), EndpointSecurityType.None);
            public static BinanceEndpointData SymbolPrice(string symbol) => new BinanceEndpointData(new Uri($"{APIPrefix}/v3/ticker/price?symbol={symbol}"), EndpointSecurityType.None);
            /// <summary>
            /// Best price/qty on the order book for all symbols.
            /// </summary>
            public static BinanceEndpointData SymbolsOrderBookTicker() => new BinanceEndpointData(new Uri($"{APIPrefix}/v3/ticker/bookTicker"), EndpointSecurityType.None);

            /// <summary>
            /// Best price/qty on the order book of a single symbol
            /// </summary>
            public static BinanceEndpointData SymbolsOrderBookTicker(string symbol) => new BinanceEndpointData(new Uri($"{APIPrefix}/v3/ticker/bookTicker?symbol={symbol}"), EndpointSecurityType.None);
        }

        public static class Account
        {
            internal static string ApiVersion = "v3";

            public static BinanceEndpointData NewOrder(CreateOrderRequest request)
            {
                var queryString = GenerateQueryStringFromData(request);
                return new BinanceEndpointData(new Uri($"{APIPrefix}/{ApiVersion}/order?{queryString}"), EndpointSecurityType.Signed);
            }
            public static BinanceEndpointData NewOrderTest(CreateOrderRequest request)
            {
                var queryString = GenerateQueryStringFromData(request);
                return new BinanceEndpointData(new Uri($"{APIPrefix}/{ApiVersion}/order/test?{queryString}"), EndpointSecurityType.Signed);
            }

            public static BinanceEndpointData QueryOrder(QueryOrderRequest request)
            {
                var queryString = GenerateQueryStringFromData(request);
                return new BinanceEndpointData(new Uri($"{APIPrefix}/{ApiVersion}/order?{queryString}"), EndpointSecurityType.Signed);
            }

            public static BinanceEndpointData CancelOrder(CancelOrderRequest request)
            {
                var queryString = GenerateQueryStringFromData(request);
                return new BinanceEndpointData(new Uri($"{APIPrefix}/{ApiVersion}/order?{queryString}"), EndpointSecurityType.Signed);
            }

            public static BinanceEndpointData CurrentOpenOrders(CurrentOpenOrdersRequest request)
            {
                var queryString = GenerateQueryStringFromData(request);
                return new BinanceEndpointData(new Uri($"{APIPrefix}/{ApiVersion}/openOrders?{queryString}"), EndpointSecurityType.Signed);
            }

            public static BinanceEndpointData AllOrders(AllOrdersRequest request)
            {
                var queryString = GenerateQueryStringFromData(request);
                return new BinanceEndpointData(new Uri($"{APIPrefix}/{ApiVersion}/allOrders?{queryString}"), EndpointSecurityType.Signed);
            }

            public static BinanceEndpointData AccountInformation => new BinanceEndpointData(new Uri($"{APIPrefix}/{ApiVersion}/account"), EndpointSecurityType.Signed);

            public static BinanceEndpointData AccountTradeList(AllTradesRequest request)
            {
                var queryString = GenerateQueryStringFromData(request);
                return new BinanceEndpointData(new Uri($"{APIPrefix}/{ApiVersion}/myTrades?{queryString}"), EndpointSecurityType.Signed);
            }

            public static BinanceEndpointData Withdraw(WithdrawRequest request)
            {
                var queryString = GenerateQueryStringFromData(request);
                return new BinanceEndpointData(new Uri($"{WAPIPrefix}/{ApiVersion}/withdraw.html?{queryString}"), EndpointSecurityType.Signed);
            }

            public static BinanceEndpointData DepositHistory(FundHistoryRequest request)
            {
                var queryString = GenerateQueryStringFromData(request);
                return new BinanceEndpointData(new Uri($"{WAPIPrefix}/{ApiVersion}/depositHistory.html?{queryString}"), EndpointSecurityType.Signed);
            }

            public static BinanceEndpointData WithdrawHistory(FundHistoryRequest request)
            {
                var queryString = GenerateQueryStringFromData(request);
                return new BinanceEndpointData(new Uri($"{WAPIPrefix}/{ApiVersion}/withdrawHistory.html?{queryString}"), EndpointSecurityType.Signed);
            }

            public static BinanceEndpointData DepositAddress(DepositAddressRequest request)
            {
                var queryString = GenerateQueryStringFromData(request);
                return new BinanceEndpointData(new Uri($"{WAPIPrefix}/{ApiVersion}/depositAddress.html?{queryString}"), EndpointSecurityType.Signed);
            }

            public static BinanceEndpointData SystemStatus()
            {
                return new BinanceEndpointData(new Uri($"{WAPIPrefix}/{ApiVersion}/systemStatus.html"), EndpointSecurityType.None);
            }
        }

        public static class Margin
        {
            public static BinanceEndpointData GetAllCrossMarginAssets() => new BinanceEndpointData(new Uri($"{APIBaseUrl2}/sapi/v1/margin/allAssets"), EndpointSecurityType.ApiKey);
            public static BinanceEndpointData GetAllCrossMarginPairs() => new BinanceEndpointData(new Uri($"{APIBaseUrl2}/sapi/v1/margin/allPairs"), EndpointSecurityType.ApiKey);
            public static BinanceEndpointData PostMarginOrder(PostMarginOrderRequest request)
            {
                var queryString = GenerateQueryStringFromData(request);
                return new BinanceEndpointData(new Uri($"{APIBaseUrl2}/sapi/v1/margin/order?{queryString}"), EndpointSecurityType.Signed);
            }
        }


        public static class Wallet
        {
            public static BinanceEndpointData DustConvert(ConvertDustRequest request)
            {
                var queryString = GenerateQueryStringFromData(request);
                return new BinanceEndpointData(new Uri($"{APIBaseUrl2}/sapi/v1/asset/dust?{queryString}"), EndpointSecurityType.Signed);
            }
        }
        private static string GenerateQueryStringFromData(IRequest request)
        {
            if (request == null)
            {
                throw new Exception("No request data provided - query string can't be created");
            }

            var obj = JObject.FromObject(request, DefaultSerializer);
            StringBuilder builder = new StringBuilder();
            foreach (var child in obj.Children().Cast<JProperty>())
            {
                if (child.Value != null)
                {
                    if (child.Value.Type == JTokenType.Array)
                    {
                        var arr = child.Value as JArray;
                        foreach (var val in arr.Values())
                            builder.Append(child.Name).Append("=").Append(System.Net.WebUtility.UrlEncode(val.ToString())).Append("&");
                    }
                    else
                        builder.Append(child.Name).Append("=").Append(System.Net.WebUtility.UrlEncode(child.Value.ToString())).Append("&");

                }
            }

            if (builder.Length > 0)
                return builder.ToString(0, builder.Length - 1);
            else
                return string.Empty;
        }
    }
  
}
