using Nethereum.ABI.FunctionEncoding.Attributes;
using System.Numerics;

namespace Application.DTOs.Subscription;

[Event("SubscriptionPurchased")]
public class SubscriptionPurchasedEventDTO : IEventDTO
{
    [Parameter("string", "userId", 1, false)]
    public string UserId { get; set; } = null!;

    [Parameter("uint8", "planType", 2, false)]
    public byte PlanType { get; set; }

    [Parameter("address", "buyer", 3, true)]
    public string Buyer { get; set; } = string.Empty;

    [Parameter("uint256", "amountPaid", 4, false)]
    public BigInteger AmountPaid { get; set; }
}