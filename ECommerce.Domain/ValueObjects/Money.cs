namespace ECommerce.Domain.ValueObjects;

public sealed record Money
{
    public decimal Amount { get; private init; }

    public string Currency { get; private init; } = string.Empty;

    private Money()
    {
    }

    public Money(decimal amount, string currency)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);

        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new ArgumentException("Currency is required.", nameof(currency));
        }

        Amount = decimal.Round(amount, 2);
        Currency = currency.ToUpperInvariant();
    }
}
