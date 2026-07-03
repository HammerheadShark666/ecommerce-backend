namespace ECommerce.Domain.ValueObjects;

public sealed record Money
{
    public decimal Amount { get; init; }

    public string Currency { get; init; } = string.Empty;

    public Money(decimal amount, string currency)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount));

        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency is required.", nameof(currency));

        Amount = decimal.Round(amount, 2);

        Currency = currency.ToUpperInvariant();
    }

    // Required by EF Core
    private Money()
    {
    }

    public override string ToString()
        => $"{Currency} {Amount:N2}";
}