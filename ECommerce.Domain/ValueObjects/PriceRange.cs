namespace ECommerce.Domain.ValueObjects;

public sealed record PriceRange
{
    public Money? Min { get; }
    public Money? Max { get; }

    private PriceRange(Money? min, Money? max)
    {
        Min = min;
        Max = max;
    }

    public static PriceRange Create(Money? min, Money? max)
    {
        ValidateOrThrow(min, max);
        return new PriceRange(min, max);
    }

    public static bool TryCreate(Money? min, Money? max, out PriceRange? range, out string? error)
    {
        try
        {
            range = Create(min, max);
            error = null;
            return true;
        }
        catch (ArgumentException ex)
        {
            range = null;
            error = ex.Message;
            return false;
        }
    }

    private static void ValidateOrThrow(Money? min, Money? max)
    {
        if (min is not null && max is not null)
        {
            if (min.Currency != max.Currency)
            {
                throw new ArgumentException("Min and Max must use the same currency.");
            }

            if (min.Amount > max.Amount)
            {
                throw new ArgumentException("Min cannot be greater than Max.");
            }
        }
    }

    public static PriceRange Unbounded => new(null, null);

    public bool Contains(Money price)
    {
        if (Min is not null && price.Currency != Min.Currency)
        {
            throw new InvalidOperationException("Currency mismatch between price and range.");
        }

        if (Max is not null && price.Currency != Max.Currency)
        {
            throw new InvalidOperationException("Currency mismatch between price and range.");
        }

        if (Min is not null && price.Amount < Min.Amount)
        {
            return false;
        }

        if (Max is not null && price.Amount > Max.Amount)
        {
            return false;
        }

        return true;
    }
}
