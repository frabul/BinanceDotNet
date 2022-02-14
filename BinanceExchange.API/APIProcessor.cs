using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BinanceExchange.API.Caching.Interfaces;
using BinanceExchange.API.Enums;
using BinanceExchange.API.Models.Response.Error;
using Newtonsoft.Json;

namespace BinanceExchange.API
{
    /// <summary>
    /// The API Processor is the chief piece of functionality responsible for handling and creating requests to the API
    /// </summary>
    public class APIProcessor : IAPIProcessor
    {
        private readonly string _apiKey;
        private readonly string _secretKey;
        private IAPICacheManager _apiCache;
        private Serilog.ILogger _logger;
        private bool _cacheEnabled;
        private TimeSpan _cacheTime;
        private readonly RequestClient _requestClient;

        public APIProcessor(string apiKey, string secretKey, IAPICacheManager apiCache, RequestClient requestClient)
        {
            _apiKey = apiKey;
            _secretKey = secretKey;
            if (apiCache != null)
            {
                _apiCache = apiCache;
                _cacheEnabled = true;
            }

            _requestClient = requestClient;
            _logger = Serilog.Log.ForContext("SourceContext","APIProcessor");
            _logger.Debug($"API Processor set up. Cache Enabled={_cacheEnabled}");
        }

        /// <summary>
        /// Set the cache expiry time
        /// </summary>
        /// <param name="time"></param>
        public void SetCacheTime(TimeSpan time)
        {
            _cacheTime = time;
        }


        Dictionary<string, string> OrdersRates = new Dictionary<string, string>();
        public string GetOrdersRate()
        {
            lock (OrdersRates)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var kv in OrdersRates)
                {
                    sb.AppendFormat("{0}: {1}", kv.Key, kv.Value);
                    sb.AppendLine();
                }
                return sb.ToString();
            }
        }

        private async Task<T> HandleResponse<T>(HttpResponseMessage message, string requestMessage, string fullCacheKey) where T : class
        {
            if (message.IsSuccessStatusCode)
            {
                //try to catch the order limit
                foreach (var header in message.Headers)
                {
                    if (header.Key.Contains("x-mbx-order-count"))
                        lock (OrdersRates)
                            OrdersRates[header.Key] = string.Join(" ", header.Value);
                }

                //decode message 
                var messageJson = await message.Content.ReadAsStringAsync();
                T messageObject = null;
                try
                {
                    messageObject = JsonConvert.DeserializeObject<T>(messageJson);
                }
                catch (Exception ex)
                {
                    string deserializeErrorMessage = $"Unable to deserialize message from: {requestMessage}. Exception: {ex.Message}";
                    _logger.Error(deserializeErrorMessage);
                    throw new BinanceException(deserializeErrorMessage, new BinanceError()
                    {
                        RequestMessage = requestMessage,
                        Message = ex.Message
                    });
                }
                //_logger.Trace($"Successful Message Response={messageJson}");

                if (messageObject == null)
                {
                    throw new Exception("Unable to deserialize to provided type");
                }
                if (_cacheEnabled)
                {
                    if (_apiCache.Contains(fullCacheKey))
                    {
                        _apiCache.Remove(fullCacheKey);
                    }
                    _apiCache.Add(messageObject, fullCacheKey, _cacheTime);
                }
                return messageObject;
            }
            var errorJson = await message.Content.ReadAsStringAsync();
            var errorObject = JsonConvert.DeserializeObject<BinanceError>(errorJson);
            if (errorObject == null) throw new BinanceException("Unexpected Error whilst handling the response", null);
            errorObject.RequestMessage = requestMessage;
            var exception = CreateBinanceException(message.StatusCode, errorObject);
            _logger.Error($"Error Message Received:{errorJson} - {errorObject}", exception);

            throw exception;
        }

        private BinanceException CreateBinanceException(HttpStatusCode statusCode, BinanceError errorObject)
        {
            if (statusCode == HttpStatusCode.GatewayTimeout)
            {
                return new BinanceTimeoutException(errorObject);
            }
            var parsedStatusCode = (int)statusCode;
            if (parsedStatusCode >= 400 && parsedStatusCode <= 500)
            {
                return new BinanceBadRequestException(errorObject);
            }
            return parsedStatusCode >= 500 ?
                new BinanceServerException(errorObject) :
                new BinanceException("Binance API Error", errorObject);
        }

        /// <summary>
        /// Checks the cache for an item, and if it exists returns it
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="partialKey">The absolute Uri of the endpoint being hit. This is used in combination with the Type name to generate a unique key</param>
        /// <param name="item"></param>
        /// <returns>Whether the item exists</returns>
        private bool CheckAndRetrieveCachedItem<T>(string fullKey, out T item) where T : class
        {
            item = null;
            var result = _apiCache.Contains(fullKey);
            item = result ? _apiCache.Get<T>(fullKey) : null;
            return result;
        }

        /// <summary>
        /// Processes a GET request
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="endpoint"></param>
        /// <param name="receiveWindow"></param>
        /// <returns></returns>
        public async Task<T> ProcessGetRequest<T>(BinanceEndpointData endpoint, int receiveWindow = 5000) where T : class
        {
            var fullKey = $"{typeof(T).Name}-{endpoint.Uri.AbsoluteUri}";
            if (_cacheEnabled && endpoint.UseCache)
            {
                if (CheckAndRetrieveCachedItem<T>(fullKey, out var item))
                {
                    return item;
                }
            }
            HttpResponseMessage message;
            switch (endpoint.SecurityType)
            {
                case EndpointSecurityType.ApiKey:
					//todo fix for margin trading
                    //var oldUri = endpoint.Uri.ToString();
                    //if (oldUri.Contains("?"))
                    //    oldUri += "&";
                    //else
                    //    oldUri += "?";
                    //message = await _requestClient.GetRequest(new Uri(oldUri + "X-MBX-APIKEY=" + _apiKey));
                    //break;
                case EndpointSecurityType.None:
                    message = await _requestClient.GetRequest(endpoint.Uri);
                    break;
                case EndpointSecurityType.Signed:
                    message = await _requestClient.SignedGetRequest(endpoint.Uri, _apiKey, _secretKey, endpoint.Uri.Query, receiveWindow);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return await HandleResponse<T>(message, endpoint.ToString(), fullKey);
        }

        /// <summary>
        /// Processes a DELETE request
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="endpoint"></param>
        /// <param name="receiveWindow"></param>
        /// <returns></returns>
        public async Task<T> ProcessDeleteRequest<T>(BinanceEndpointData endpoint, int receiveWindow = 5000) where T : class
        {
            var fullKey = $"{typeof(T).Name}-{endpoint.Uri.AbsoluteUri}";
            if (_cacheEnabled && endpoint.UseCache)
            {
                T item;
                if (CheckAndRetrieveCachedItem<T>(fullKey, out item))
                {
                    return item;
                }
            }
            HttpResponseMessage message;
            switch (endpoint.SecurityType)
            {
                case EndpointSecurityType.ApiKey:
                    message = await _requestClient.DeleteRequest(endpoint.Uri);
                    break;
                case EndpointSecurityType.Signed:
                    message = await _requestClient.SignedDeleteRequest(endpoint.Uri, _apiKey, _secretKey, endpoint.Uri.Query, receiveWindow);
                    break;
                case EndpointSecurityType.None:
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return await HandleResponse<T>(message, endpoint.ToString(), fullKey);
        }

        /// <summary>
        /// Processes a POST request
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="endpoint"></param>
        /// <param name="receiveWindow"></param>
        /// <returns></returns>
        public async Task<T> ProcessPostRequest<T>(BinanceEndpointData endpoint, int receiveWindow = 5000) where T : class
        {
            var fullKey = $"{typeof(T).Name}-{endpoint.Uri.AbsoluteUri}";
            if (_cacheEnabled && endpoint.UseCache)
            {
                T item;
                if (CheckAndRetrieveCachedItem<T>(fullKey, out item))
                {
                    return item;
                }
            }
            HttpResponseMessage message;
            switch (endpoint.SecurityType)
            {
                case EndpointSecurityType.ApiKey:
                    message = await _requestClient.PostRequest(endpoint.Uri);
                    break;
                case EndpointSecurityType.None:
                    throw new ArgumentOutOfRangeException();
                case EndpointSecurityType.Signed:
                    message = await _requestClient.SignedPostRequest(endpoint.Uri, _apiKey, _secretKey, endpoint.Uri.Query, receiveWindow);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return await HandleResponse<T>(message, endpoint.ToString(), fullKey);
        }

        /// <summary>
        /// Processes a PUT request
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="endpoint"></param>
        /// <param name="receiveWindow"></param>
        /// <returns></returns>
        public async Task<T> ProcessPutRequest<T>(BinanceEndpointData endpoint, int receiveWindow = 5000) where T : class
        {
            var fullKey = $"{typeof(T).Name}-{endpoint.Uri.AbsoluteUri}";
            if (_cacheEnabled && endpoint.UseCache)
            {
                T item;
                if (CheckAndRetrieveCachedItem<T>(fullKey, out item))
                {
                    return item;
                }
            }
            HttpResponseMessage message;
            switch (endpoint.SecurityType)
            {
                case EndpointSecurityType.ApiKey:
                    message = await _requestClient.PutRequest(endpoint.Uri);
                    break;
                case EndpointSecurityType.None:
                case EndpointSecurityType.Signed:
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return await HandleResponse<T>(message, endpoint.ToString(), fullKey);
        }
    }
}