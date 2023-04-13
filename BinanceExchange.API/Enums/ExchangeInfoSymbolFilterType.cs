using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace BinanceExchange.API.Enums
{
    public enum ExchangeInfoSymbolFilterType
    {
        [EnumMember(Value = "PRICE_FILTER")]
        PriceFilter,
        [EnumMember(Value = "LOT_SIZE")]
        LotSize,
        [EnumMember(Value = "MIN_NOTIONAL")]
        MinNotional,
        [EnumMember(Value = "MAX_NUM_ALGO_ORDERS")]
        MaxNumAlgoOrders,
        [EnumMember(Value = "ICEBERG_PARTS")]
        IcebergParts,
        [EnumMember(Value = "PERCENT_PRICE")] 
        PercentPrice,
        [EnumMember(Value = "MARKET_LOT_SIZE")]
        MarketLotSize,
        [EnumMember(Value = "MAX_NUM_ORDERS")]
        MaxNumOrders,
        [EnumMember(Value = "MAX_POSITION")]
        MaxPosition,
        [EnumMember(Value = "TRAILING_DELTA")]
        TrailingDelta,
        [EnumMember(Value = "PERCENT_PRICE_BY_SIDE")]
        PercentPriceBySide,
        [EnumMember(Value = "NOTIONAL")]
        Notional,
        UnknownFilter,
    }
}