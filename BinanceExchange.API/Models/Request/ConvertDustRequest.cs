using BinanceExchange.API.Models.Request.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BinanceExchange.API.Models.Request
{
    public class ConvertDustRequest : IRequest
    {
        [JsonProperty("asset")]
        public string[] Asset { get; set; }
        public ConvertDustRequest(IEnumerable<string> assets)
        {
            Asset = assets.ToArray();
        }
    } 
}
