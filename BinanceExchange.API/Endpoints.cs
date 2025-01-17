using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using BinanceExchange.API.Converter;
using BinanceExchange.API.Enums;
using BinanceExchange.API.Extensions;
using BinanceExchange.API.Models.Request;
using BinanceExchange.API.Models.Request.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace BinanceExchange.API
{
    using static Endpoints;
    public class Endpoints
    {

        private static readonly JsonSerializerSettings _settings = new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore,
            FloatParseHandling = FloatParseHandling.Decimal
        };
        private static readonly JsonSerializer DefaultSerializer = JsonSerializer.Create(_settings);
        private ApiAddresses Addresses { get; }

        /// <summary>
        /// Defaults to API binance domain (https)
        /// </summary>
        internal string RestApiDomain => Addresses.RestApiAddresses.First();

        /// <summary>
        /// Defaults to WAPI binance domain (https)
        /// </summary>
        private string WAPIPrefix => $"{RestApiDomain}/wapi";

        private string APIPrefix => $"{RestApiDomain}/api";
        public UserStream UserStream { get; }
        public General General { get; }
        public MarketData MarketData { get; }
        public Account Account { get; }
        public Margin Margin { get; }
        public Wallet Wallet { get; }
        public Other Other { get; }

        public Endpoints(ApiAddresses addresses)
        {
            Addresses = addresses;
            UserStream = new UserStream
            {
                APIPrefix = APIPrefix
            };
            General = new General
            {
                APIPrefix = APIPrefix
            };
            MarketData = new MarketData
            {
                MarketDataApiBaseUrl = $"{Addresses.PublicDataAddress}/api"
            };
            Account = new Account
            {
                APIPrefix = APIPrefix,
                WAPIPrefix = WAPIPrefix,
            };
            Margin = new Margin
            {
                RestApiDomain = RestApiDomain
            };
            Wallet = new Wallet
            {
                RestApiDomain = RestApiDomain
            };
        }


        public static string GenerateQueryStringFromData(IRequest request)
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
    public class UserStream
    {
        public string APIPrefix { get; set; }
        internal static string ApiVersion = "v3";

        /// <summary>
        /// Start a user data stream
        /// </summary>
        public BinanceEndpointData StartUserDataStream => new BinanceEndpointData(new Uri($"{APIPrefix}/{ApiVersion}/userDataStream"), EndpointSecurityType.ApiKey);

        /// <summary>
        /// Ping a user data stream to prevent a timeout
        /// </summary>
        public BinanceEndpointData KeepAliveUserDataStream(string listenKey)
        {
            return new BinanceEndpointData(new Uri($"{APIPrefix}/v3/userDataStream?listenKey={listenKey}"),
                EndpointSecurityType.ApiKey);
        }

        /// <summary>
        /// Close a user data stream to prevent
        /// </summary>
        public BinanceEndpointData CloseUserDataStream(string listenKey)
        {
            return new BinanceEndpointData(new Uri($"{APIPrefix}/{ApiVersion}/userDataStream?listenKey={listenKey}"),
                EndpointSecurityType.ApiKey);
        }
    }

    public class General
    {
        public string APIPrefix { get; set; }
        internal static string ApiVersion = "v3";

        /// <summary>
        /// Test connectivity to the Rest API.
        /// </summary>
        public BinanceEndpointData TestConnectivity => new BinanceEndpointData(new Uri($"{APIPrefix}/{ApiVersion}/ping"), EndpointSecurityType.None);

        /// <summary>
        /// Test connectivity to the Rest API and get the current server time.
        /// </summary>
        public BinanceEndpointData ServerTime => new BinanceEndpointData(new Uri($"{APIPrefix}/{ApiVersion}/time"), EndpointSecurityType.None);

        /// <summary>
        /// Current exchange trading rules and symbol information.
        /// </summary>
        public BinanceEndpointData ExchangeInfo => new BinanceEndpointData(new Uri($"{APIPrefix}/{ApiVersion}/exchangeInfo"), EndpointSecurityType.None);

    }

    public class MarketData
    {
        public string MarketDataApiBaseUrl { get; set; }
        internal static string ApiVersion = "v3";
        /// <summary>
        /// Gets the order book with all bids and asks
        /// </summary>
        public BinanceEndpointData OrderBook(string symbol, int limit, bool useCache = false)
        {
            return new BinanceEndpointData(new Uri($"{MarketDataApiBaseUrl}/{ApiVersion}/depth?symbol={symbol}&limit={limit}"), EndpointSecurityType.None, useCache);
        }

        /// <summary>
        /// Get compressed, aggregate trades. Trades that fill at the time, from the same order, with the same price will have the quantity aggregated.
        /// </summary>
        public BinanceEndpointData CompressedAggregateTrades(GetCompressedAggregateTradesRequest request)
        {
            var queryString = GenerateQueryStringFromData(request);
            return new BinanceEndpointData(new Uri($"{MarketDataApiBaseUrl}/{ApiVersion}/aggTrades?{queryString}"), EndpointSecurityType.None);
        }

        /// <summary>
        /// Kline/candlestick bars for a symbol. Klines are uniquely identified by their open time.
        /// </summary>
        public BinanceEndpointData KlineCandlesticks(GetKlinesCandlesticksRequest request)
        {
            var queryString = GenerateQueryStringFromData(request);
            return new BinanceEndpointData(new Uri($"{MarketDataApiBaseUrl}/{ApiVersion}/klines?{queryString}"), EndpointSecurityType.None);
        }

        /// <summary>
        /// 24 hour price change statistics.
        /// </summary>
        public BinanceEndpointData DayPriceTicker(string symbol)
        {
            return new BinanceEndpointData(new Uri($"{MarketDataApiBaseUrl}/{ApiVersion}/ticker/24hr?symbol={symbol}"),
                EndpointSecurityType.None);
        }

        /// <summary>
        /// Latest price for all symbols.
        /// </summary>
        public BinanceEndpointData AllSymbolsPriceTicker => new BinanceEndpointData(new Uri($"{MarketDataApiBaseUrl}/v3/ticker/price"), EndpointSecurityType.None);
        public BinanceEndpointData SymbolPrice(string symbol) => new BinanceEndpointData(new Uri($"{MarketDataApiBaseUrl}/v3/ticker/price?symbol={symbol}"), EndpointSecurityType.None);
        /// <summary>
        /// Best price/qty on the order book for all symbols.
        /// </summary>
        public BinanceEndpointData SymbolsOrderBookTicker() => new BinanceEndpointData(new Uri($"{MarketDataApiBaseUrl}/v3/ticker/bookTicker"), EndpointSecurityType.None);

        /// <summary>
        /// Best price/qty on the order book of a single symbol
        /// </summary>
        public BinanceEndpointData SymbolsOrderBookTicker(string symbol) => new BinanceEndpointData(new Uri($"{MarketDataApiBaseUrl}/v3/ticker/bookTicker?symbol={symbol}"), EndpointSecurityType.None);
    }

    public class Account
    {
        public string APIPrefix { get; set; }
        public string WAPIPrefix { get; set; }
        internal static string ApiVersion = "v3";

        public BinanceEndpointData NewOrder(CreateOrderRequest request)
        {
            var queryString = GenerateQueryStringFromData(request);
            return new BinanceEndpointData(new Uri($"{APIPrefix}/{ApiVersion}/order?{queryString}"), EndpointSecurityType.Signed);
        }
        public BinanceEndpointData NewOrderTest(CreateOrderRequest request)
        {
            var queryString = GenerateQueryStringFromData(request);
            return new BinanceEndpointData(new Uri($"{APIPrefix}/{ApiVersion}/order/test?{queryString}"), EndpointSecurityType.Signed);
        }

        public BinanceEndpointData QueryOrder(QueryOrderRequest request)
        {
            var queryString = GenerateQueryStringFromData(request);
            return new BinanceEndpointData(new Uri($"{APIPrefix}/{ApiVersion}/order?{queryString}"), EndpointSecurityType.Signed);
        }

        public BinanceEndpointData CancelOrder(CancelOrderRequest request)
        {
            var queryString = GenerateQueryStringFromData(request);
            return new BinanceEndpointData(new Uri($"{APIPrefix}/{ApiVersion}/order?{queryString}"), EndpointSecurityType.Signed);
        }

        public BinanceEndpointData CurrentOpenOrders(CurrentOpenOrdersRequest request)
        {
            var queryString = GenerateQueryStringFromData(request);
            return new BinanceEndpointData(new Uri($"{APIPrefix}/{ApiVersion}/openOrders?{queryString}"), EndpointSecurityType.Signed);
        }

        public BinanceEndpointData AllOrders(AllOrdersRequest request)
        {
            var queryString = GenerateQueryStringFromData(request);
            return new BinanceEndpointData(new Uri($"{APIPrefix}/{ApiVersion}/allOrders?{queryString}"), EndpointSecurityType.Signed);
        }

        public BinanceEndpointData AccountInformation(bool omitZeroBalance) => new BinanceEndpointData(new Uri($"{APIPrefix}/{ApiVersion}/account?omitZeroBalances={omitZeroBalance.ToString().ToLower()}"), EndpointSecurityType.Signed);

        public BinanceEndpointData AccountTradeList(AllTradesRequest request)
        {
            var queryString = GenerateQueryStringFromData(request);
            return new BinanceEndpointData(new Uri($"{APIPrefix}/{ApiVersion}/myTrades?{queryString}"), EndpointSecurityType.Signed);
        }

        public BinanceEndpointData Withdraw(WithdrawRequest request)
        {
            var queryString = GenerateQueryStringFromData(request);
            return new BinanceEndpointData(new Uri($"{WAPIPrefix}/{ApiVersion}/withdraw.html?{queryString}"), EndpointSecurityType.Signed);
        }

        public BinanceEndpointData DepositHistory(FundHistoryRequest request)
        {
            var queryString = GenerateQueryStringFromData(request);
            return new BinanceEndpointData(new Uri($"{WAPIPrefix}/{ApiVersion}/depositHistory.html?{queryString}"), EndpointSecurityType.Signed);
        }

        public BinanceEndpointData WithdrawHistory(FundHistoryRequest request)
        {
            var queryString = GenerateQueryStringFromData(request);
            return new BinanceEndpointData(new Uri($"{WAPIPrefix}/{ApiVersion}/withdrawHistory.html?{queryString}"), EndpointSecurityType.Signed);
        }

        public BinanceEndpointData DepositAddress(DepositAddressRequest request)
        {
            var queryString = GenerateQueryStringFromData(request);
            return new BinanceEndpointData(new Uri($"{WAPIPrefix}/{ApiVersion}/depositAddress.html?{queryString}"), EndpointSecurityType.Signed);
        }

        public BinanceEndpointData SystemStatus()
        {
            return new BinanceEndpointData(new Uri($"{WAPIPrefix}/{ApiVersion}/systemStatus.html"), EndpointSecurityType.None);
        }
    }

    public class Margin
    {
        public string RestApiDomain { get; set; }
        public BinanceEndpointData GetAllCrossMarginAssets() => new BinanceEndpointData(new Uri($"{RestApiDomain}/sapi/v1/margin/allAssets"), EndpointSecurityType.ApiKey);
        public BinanceEndpointData GetAllCrossMarginPairs() => new BinanceEndpointData(new Uri($"{RestApiDomain}/sapi/v1/margin/allPairs"), EndpointSecurityType.ApiKey);
        public BinanceEndpointData PostMarginOrder(PostMarginOrderRequest request)
        {
            var queryString = GenerateQueryStringFromData(request);
            return new BinanceEndpointData(new Uri($"{RestApiDomain}/sapi/v1/margin/order?{queryString}"), EndpointSecurityType.Signed);
        }
    }


    public class Wallet
    {
        public string RestApiDomain { get; set; }
        public BinanceEndpointData DustConvert(ConvertDustRequest request)
        {
            var queryString = GenerateQueryStringFromData(request);
            return new BinanceEndpointData(new Uri($"{RestApiDomain}/sapi/v1/asset/dust?{queryString}"), EndpointSecurityType.Signed);
        }
        public BinanceEndpointData GetDustAssets()
        {
            return new BinanceEndpointData(new Uri($"{RestApiDomain}/sapi/v1/asset/dust-btc"), EndpointSecurityType.Signed);
        }
    }

    public class Other
    {
        public string RestApiDomain { get; set; }
        public BinanceEndpointData GetDelistSchedule(DateTime time)
        {
            var timestamp = time.ConvertToUnixTime();
            return new BinanceEndpointData(new Uri($"{RestApiDomain}/sapi/v1/spot/delist-schedule?timestamp={timestamp}"), EndpointSecurityType.Signed);
        }

    }

    public class ApiAddresses
    {
        public string[] RestApiAddresses { get; private set; }
        public string PublicDataAddress { get; private set; }
        public string WebsocketUriPrefix { get; private set; }
        public string CombinedWebsocketUriPrefix { get; private set; }

        public static ApiAddresses MainNet = new ApiAddresses
        {
            RestApiAddresses = new string[] {
                "https://api.binance.com",
                "https://api-gcp.binance.com",
                "https://api1.binance.com",
                "https://api2.binance.com",
                "https://api3.binance.com",
                "https://api4.binance.com",
            },
            PublicDataAddress = "https://data-api.binance.vision",
            WebsocketUriPrefix = "wss://stream.binance.com:9443/ws",
            CombinedWebsocketUriPrefix = "wss://stream.binance.com:9443/stream?streams=",
        };
        
        public static ApiAddresses TestNet = new ApiAddresses
        {
            RestApiAddresses = new string[] {
                "https://testnet.binance.vision"
            },
            PublicDataAddress = "https://data-api.binance.vision",
            WebsocketUriPrefix = "wss://testnet.binance.vision/ws",
            CombinedWebsocketUriPrefix = "wss://testnet.binance.vision/stream?streams=",

        };
    }

}
