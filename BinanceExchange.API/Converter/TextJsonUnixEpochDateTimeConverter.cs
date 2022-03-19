using BinanceExchange.API.Enums;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BinanceExchange.API.Converter
{
    /// <summary>
    /// Used for deserializing json with Microsoft date format.
    /// </summary>
    internal sealed class TextJsonUnixEpochDateTimeConverter : JsonConverter<DateTime>
    {
        private static readonly DateTime EpochDateTime = new DateTime(1970, 1, 1, 0, 0, 0);

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TryGetInt64(out long x))
                return EpochDateTime.AddMilliseconds(x);
            throw (new Exception("TextJsonUnixEpochDateTimeConverter expected an integer token"));

        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
    internal sealed class TextJsonStringToDecimal : JsonConverter<decimal>
    {
        private static readonly DateTime EpochDateTime = new DateTime(1970, 1, 1, 0, 0, 0);

        public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var str = reader.GetString();
            if (str != null)
                return Convert.ToDecimal(str);
            throw (new Exception("TextJsonUnixEpochDateTimeConverter expected an integer token"));

        }

        public override void Write(Utf8JsonWriter writer, decimal val, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }

    internal sealed class TextJsonKlineTimeframeConverter : JsonConverter<KlineInterval>
    {


        public override KlineInterval Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var str = reader.GetString();
            if (str != null)
            {

                switch (str)
                {
                    case "1m": return KlineInterval.OneMinute;
                    case "3m": return KlineInterval.ThreeMinutes;
                    case "5m": return KlineInterval.FiveMinutes;
                    case "15m": return KlineInterval.FifteenMinutes;
                    case "30m": return KlineInterval.ThirtyMinutes;
                    case "1h": return KlineInterval.OneHour;
                    case "2h": return KlineInterval.TwoHours;
                    case "4h": return KlineInterval.FourHours;
                    case "6h": return KlineInterval.SixHours;
                    case "8h": return KlineInterval.EightHours;
                    case "12h": return KlineInterval.TwelveHours;
                    case "1d": return KlineInterval.OneDay;
                    case "3d": return KlineInterval.ThreeDays;
                    case "1w": return KlineInterval.OneWeek;
                    case "1M": return KlineInterval.OneMonth;
                    default: throw new NotImplementedException($"Enum member not implemented for {str}");
                }
            }
            throw (new Exception("TextJsonUnixEpochDateTimeConverter expected an integer token"));

        }

        public override void Write(Utf8JsonWriter writer, KlineInterval value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}