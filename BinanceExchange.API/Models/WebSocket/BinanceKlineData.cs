using System;
using System.Runtime.Serialization;
using BinanceExchange.API.Converter;
using BinanceExchange.API.Models.WebSocket.Interfaces;
using Newtonsoft.Json;

namespace BinanceExchange.API.Models.WebSocket
{
    /// <summary>
    /// Data returned from the Binance WebSocket Kline endpoint
    /// </summary>
    [DataContract]
    public class BinanceKlineData : ISymbolWebSocketResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("e")]
        [JsonProperty(PropertyName = "e")]
        [DataMember(Order = 1)]
        public string EventType { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("E")]
        [System.Text.Json.Serialization.JsonConverter(typeof(TextJsonUnixEpochDateTimeConverter))]
        [JsonProperty(PropertyName = "E")]
        [DataMember(Order = 2)]
        [JsonConverter(typeof(EpochTimeConverter))]
        public DateTime EventTime { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("s")]
        [JsonProperty(PropertyName = "s")]
        [DataMember(Order = 3)]
        public string Symbol { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("k")]
        [JsonProperty(PropertyName = "k")]
        [DataMember(Order = 4)]
        public BinanceKline Kline { get; set; }
    }
}
