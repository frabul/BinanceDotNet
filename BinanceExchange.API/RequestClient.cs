using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinanceExchange.API.Enums;
using BinanceExchange.API.Extensions; 
namespace BinanceExchange.API
{
    public class RequestClient
    {
        private readonly HttpClient _httpClient;
        private const string APIHeader = "X-MBX-APIKEY";
        private TimeSpan _timestampOffset;
        private Serilog.ILogger _logger;
        private readonly object LockObject = new object();

        public RequestClient()
        {
            ServicePointManager.DefaultConnectionLimit = 500;
            var httpClientHandler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip
            };
            _httpClient = new HttpClient(httpClientHandler);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.Timeout = TimeSpan.FromSeconds(4);

#if NETCORE
            httpClientHandler.MaxConnectionsPerServer = 500;
#endif
            _logger = Serilog.Log.ForContext("SourceContext", "BinanceExchange.API.RequestClient");
        }

        /// <summary>
        /// Used to adjust the client timestamp
        /// </summary>
        /// <param name="time">TimeSpan to adjust timestamp by</param>
        public void SetTimestampOffset(TimeSpan time)
        {
            _timestampOffset = time;
            _logger.Verbose($"Timestamp offset is now : {time}");
        }

        /// <summary>
        /// Assigns a new seconds limit
        /// </summary>
        /// <param name="key">Your API Key</param>
        public void SetAPIKey(string key)
        {
            if (_httpClient.DefaultRequestHeaders.Contains(APIHeader))
            {
                lock (LockObject)
                {
                    if (_httpClient.DefaultRequestHeaders.Contains(APIHeader))
                    {
                        _httpClient.DefaultRequestHeaders.Remove(APIHeader);
                    }
                }
            }
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation(APIHeader, new[] { key });
        }

        /// <summary>
        /// Create a generic GetRequest to the specified endpoint
        /// </summary>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> GetRequest(Uri endpoint)
        {
            _logger.Verbose($"Creating a GET Request to {endpoint.AbsoluteUri}");
            return await CreateRequest(endpoint);
        }

        /// <summary>
        /// Creates a generic GET request that is signed
        /// </summary>s
        /// <param name="endpoint"></param>
        /// <param name="apiKey"></param>
        /// <param name="secretKey"></param>
        /// <param name="signatureRawData"></param>
        /// <param name="receiveWindow"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> SignedGetRequest(Uri endpoint, string apiKey, string secretKey, string signatureRawData, long receiveWindow = 5000)
        {
            _logger.Verbose($"Creating a SIGNED GET Request to {endpoint.AbsoluteUri}");
            var uri = CreateValidUri(endpoint, secretKey, signatureRawData, receiveWindow);
            _logger.Verbose($"Concat URL for request: {uri.AbsoluteUri}");
            return await CreateRequest(uri, HttpVerb.GET);
        }

        /// <summary>
        /// Create a generic PostRequest to the specified endpoint
        /// </summary>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> PostRequest(Uri endpoint)
        {
            _logger.Verbose($"Creating a POST Request to {endpoint.AbsoluteUri}");
            return await CreateRequest(endpoint, HttpVerb.POST);
        }

        /// <summary>
        /// Create a generic DeleteRequest to the specified endpoint
        /// </summary>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> DeleteRequest(Uri endpoint)
        {
            _logger.Verbose($"Creating a DELETE Request to {endpoint.AbsoluteUri}");
            return await CreateRequest(endpoint, HttpVerb.DELETE);
        }

        /// <summary>
        /// Create a generic PutRequest to the specified endpoint
        /// </summary>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> PutRequest(Uri endpoint)
        {
            _logger.Verbose($"Creating a PUT Request to {endpoint.AbsoluteUri}");
            return await CreateRequest(endpoint, HttpVerb.PUT);
        }

        /// <summary>
        /// Creates a generic GET request that is signed
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="apiKey"></param>
        /// <param name="secretKey"></param>
        /// <param name="signatureRawData"></param>
        /// <param name="receiveWindow"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> SignedPostRequest(Uri endpoint, string apiKey, string secretKey, string signatureRawData, long receiveWindow = 5000)
        {
            _logger.Verbose($"Creating a SIGNED POST Request to {endpoint.AbsoluteUri}");
            var uri = CreateValidUri(endpoint, secretKey, signatureRawData, receiveWindow);
            return await CreateRequest(uri, HttpVerb.POST);
        }

        /// <summary>
        /// Creates a generic DELETE request that is signed
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="apiKey"></param>
        /// <param name="secretKey"></param>
        /// <param name="signatureRawData"></param>
        /// <param name="receiveWindow"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> SignedDeleteRequest(Uri endpoint, string apiKey, string secretKey, string signatureRawData, long receiveWindow = 5000)
        {
            _logger.Verbose($"Creating a SIGNED DELETE Request to {endpoint.AbsoluteUri}");
            var uri = CreateValidUri(endpoint, secretKey, signatureRawData, receiveWindow);
            return await CreateRequest(uri, HttpVerb.DELETE);
        }


        /// <summary>
        /// Creates a valid Uri with signature
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="secretKey"></param>
        /// <param name="signatureRawData"></param>
        /// <param name="receiveWindow"></param>
        /// <returns></returns>
        /// 
        private Uri CreateValidUri(Uri endpoint, string secretKey, string signatureRawData, long receiveWindow)
        {
            string timestamp;
#if NETSTANDARD2_0
            timestamp = DateTimeOffset.UtcNow.AddMilliseconds(_timestampOffset.TotalMilliseconds).ToUnixTimeMilliseconds().ToString();
#else
            timestamp = DateTime.UtcNow.AddMilliseconds(_timestampOffset.TotalMilliseconds).ConvertToUnixTime().ToString();
#endif
            var qsDataProvided = !string.IsNullOrEmpty(signatureRawData);
            var argEnding = $"timestamp={timestamp}&recvWindow={receiveWindow}";
            var adjustedSignature = !string.IsNullOrEmpty(signatureRawData) ? $"{signatureRawData.Substring(1)}&{argEnding}" : $"{argEnding}";
            var hmacResult = CreateHMACSignature(secretKey, adjustedSignature);
            var connector = !qsDataProvided ? "?" : "&";
            var uri = new Uri($"{endpoint}{connector}{argEnding}&signature={hmacResult}");
            return uri;
        }

        /// <summary>
        /// Creates a HMACSHA256 Signature based on the key and total parameters
        /// </summary>
        /// <param name="key">The secret key</param>
        /// <param name="totalParams">URL Encoded values that would usually be the query string for the request</param>
        /// <returns></returns>
        private string CreateHMACSignature(string key, string totalParams)
        {
            var messageBytes = Encoding.UTF8.GetBytes(totalParams);
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var hash = new HMACSHA256(keyBytes);
            var computedHash = hash.ComputeHash(messageBytes);
            return BitConverter.ToString(computedHash).Replace("-", "").ToLower();
        }

        /// <summary>
        /// Makes a request to the specifed Uri, only if it hasn't exceeded the call limit 
        /// </summary>
        /// <param name="endpoint">Endpoint to request</param>
        /// <param name="verb"></param>
        /// <returns></returns>
        private async Task<HttpResponseMessage> CreateRequest(Uri endpoint, HttpVerb verb = HttpVerb.GET)
        {
            Task<HttpResponseMessage> task = null;
            switch (verb)
            {
                case HttpVerb.GET:
                    task = _httpClient.GetAsync(endpoint);
                    break;
                case HttpVerb.POST:
                    task = _httpClient.PostAsync(endpoint, null);
                    break;
                case HttpVerb.DELETE:
                    task = _httpClient.DeleteAsync(endpoint);
                    break;
                case HttpVerb.PUT:
                    task = _httpClient.PutAsync(endpoint, null);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(verb), verb, null);
            }
            return await task;
        }

        public void SetLogger(Serilog.ILogger logger)
        {
            _logger = logger;
        }
    }
}
