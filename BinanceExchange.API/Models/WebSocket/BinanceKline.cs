using System;
using System.Runtime.Serialization;
using BinanceExchange.API.Converter;
using BinanceExchange.API.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BinanceExchange.API.Models.WebSocket
{
    [DataContract]
    public class BinanceKline
    {
        [System.Text.Json.Serialization.JsonPropertyName("t")]
        [System.Text.Json.Serialization.JsonConverter(typeof(TextJsonUnixEpochDateTimeConverter))]
        [JsonProperty(PropertyName = "t")]
        [DataMember(Order = 1)]
        [JsonConverter(typeof(EpochTimeConverter))]
        public DateTime StartTime { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("T")]
        [System.Text.Json.Serialization.JsonConverter(typeof(TextJsonUnixEpochDateTimeConverter))]
        [JsonProperty(PropertyName = "T")]
        [DataMember(Order = 2)]
        [JsonConverter(typeof(EpochTimeConverter))]
        public DateTime EndTime { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("s")]
        [JsonProperty(PropertyName = "s")]
        [DataMember(Order = 3)]
        public string Symbol { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("i")]
        [System.Text.Json.Serialization.JsonConverter(typeof(TextJsonKlineTimeframeConverter))]

        [JsonProperty(PropertyName = "i")]
        [DataMember(Order = 4)]
        [JsonConverter(typeof(StringEnumConverter))]
        public KlineInterval Interval { get; set; }



        
        [System.Text.Json.Serialization.JsonPropertyName("f")]
        [JsonProperty(PropertyName = "f")]
        [DataMember(Order = 5)]
        public long FirstTradeId { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("L")]
        [JsonProperty(PropertyName = "L")]
        [DataMember(Order = 6)]
        public long LastTradeId { get; set; }

        [System.Text.Json.Serialization.JsonConverter(typeof(TextJsonStringToDecimal))]
        [System.Text.Json.Serialization.JsonPropertyName("o")]
        [JsonProperty(PropertyName = "o")]
        [DataMember(Order = 7)]
        public decimal Open { get; set; }

        [System.Text.Json.Serialization.JsonConverter(typeof(TextJsonStringToDecimal))]
        [System.Text.Json.Serialization.JsonPropertyName("c")]
        [JsonProperty(PropertyName = "c")]
        [DataMember(Order = 8)]
        public decimal Close { get; set; }

        [System.Text.Json.Serialization.JsonConverter(typeof(TextJsonStringToDecimal))]
        [System.Text.Json.Serialization.JsonPropertyName("h")]
        [JsonProperty(PropertyName = "h")]
        [DataMember(Order = 9)]
        public decimal High { get; set; }

        [System.Text.Json.Serialization.JsonConverter(typeof(TextJsonStringToDecimal))]
        [System.Text.Json.Serialization.JsonPropertyName("l")]
        [JsonProperty(PropertyName = "l")]
        [DataMember(Order = 10)]
        public decimal Low { get; set; }

        [System.Text.Json.Serialization.JsonConverter(typeof(TextJsonStringToDecimal))]
        [System.Text.Json.Serialization.JsonPropertyName("v")]
        [JsonProperty(PropertyName = "v")]
        [DataMember(Order = 11)]
        public decimal Volume { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("n")]
        [JsonProperty(PropertyName = "n")]
        [DataMember(Order = 12)]
        public int NumberOfTrades { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("x")]
        [JsonProperty(PropertyName = "x")]
        [DataMember(Order = 13)]
        public bool IsBarFinal { get; set; }

        [System.Text.Json.Serialization.JsonConverter(typeof(TextJsonStringToDecimal))]
        [System.Text.Json.Serialization.JsonPropertyName("q")]
        [JsonProperty(PropertyName = "q")]
        [DataMember(Order = 14)]
        public decimal QuoteVolume { get; set; }

        [System.Text.Json.Serialization.JsonConverter(typeof(TextJsonStringToDecimal))]
        [System.Text.Json.Serialization.JsonPropertyName("V")]
        [JsonProperty(PropertyName = "V")]
        [DataMember(Order = 15)]
        public decimal VolumeOfActivyBuy { get; set; }

        [System.Text.Json.Serialization.JsonConverter(typeof(TextJsonStringToDecimal))]
        [System.Text.Json.Serialization.JsonPropertyName("Q")]
        [JsonProperty(PropertyName = "Q")]
        [DataMember(Order = 16)]
        public decimal QuoteVolumeOfActivyBuy { get; set; }
    }
}