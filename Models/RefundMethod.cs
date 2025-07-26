using System.Text.Json.Serialization;
namespace ShopAI.Models
{
    // Enum representing available refund methods.
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum RefundMethod
    {
        Wallet,   
        JazzCash,
        PayFast,
        Visa,
        Master,
        EasyPaisa
    }
}