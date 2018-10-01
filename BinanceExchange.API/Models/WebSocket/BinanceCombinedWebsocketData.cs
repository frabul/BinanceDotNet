using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace BinanceExchange.API.Models.WebSocket
{
    [DataContract]
    public class BinanceCombinedWebsocketData
    {
        [JsonProperty("stream")]
        public string StreamName { get; set; } 
        [JsonProperty("data")]
        public JObject RawData { get; set; }
    }
}
