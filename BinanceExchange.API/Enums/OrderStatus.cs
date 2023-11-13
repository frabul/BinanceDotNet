using System.Runtime.Serialization;

namespace BinanceExchange.API.Enums
{
    public enum OrderStatus
    {
        [EnumMember(Value = "NEW")]
        New,
        [EnumMember(Value = "PARTIALLY_FILLED")]
        PartiallyFilled,
        [EnumMember(Value = "FILLED")]
        Filled,
        [EnumMember(Value = "CANCELED")]
        Cancelled,
        [EnumMember(Value = "PENDING_CANCEL")]
        PendingCancel,
        [EnumMember(Value = "REJECTED")]
        Rejected,
        [EnumMember(Value = "EXPIRED")]
        Expired,
        [EnumMember(Value = "EXPIRED_IN_MATCH")]
        /// <summary> The order was canceled by the exchange due to STP trigger. (e.g. an order with EXPIRE_TAKER will match with existing orders on the book with the same account or same tradeGroupId) </summary>
        ExpiredInMatch
    }
}