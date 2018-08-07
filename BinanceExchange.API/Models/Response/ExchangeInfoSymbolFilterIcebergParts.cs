using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace BinanceExchange.API.Models.Response
{
    [DataContract]
    public class ExchangeInfoSymbolFilterIcebergParts : ExchangeInfoSymbolFilter 
    {
        [JsonProperty("limit")]
        public int Limit { get; set; }
    }
}
