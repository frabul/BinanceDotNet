using System;

namespace BinanceExchange.API.Client
{
    public class ClientConfiguration
    {
        public string ApiKey { get; set; }
        public string SecretKey { get; set; }
        public bool EnableRateLimiting { get; set; }
        public TimeSpan CacheTime { get; set; } = TimeSpan.FromMinutes(30);
        public TimeSpan TimestampOffset { get; set; } = TimeSpan.FromMilliseconds(0);
        public Serilog.ILogger Logger { get; set; }
        public int DefaultReceiveWindow { get; set; } = 5000;
        public double RateLimitFactor { get; set; } = 0.6;
    }
}