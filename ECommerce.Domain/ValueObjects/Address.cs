namespace ECommerce.Domain.ValueObjects;

public sealed record Address
{
    public string Line1 { get; init; } = string.Empty;

    public string? Line2 { get; init; }

    public string City { get; init; } = string.Empty;

    public string County { get; init; } = string.Empty;

    public string PostCode { get; init; } = string.Empty;

    public string Country { get; init; } = string.Empty;

    public Address(
        string line1,
        string? line2,
        string city,
        string county,
        string postCode,
        string country)
    {
        if (string.IsNullOrWhiteSpace(line1))
        {
            throw new ArgumentException("Street is required.");
        }

        if (string.IsNullOrWhiteSpace(city))
        {
            throw new ArgumentException("City is required.");
        }

        if (string.IsNullOrWhiteSpace(postCode))
        {
            throw new ArgumentException("Postcode is required.");
        }

        if (string.IsNullOrWhiteSpace(country))
        {
            throw new ArgumentException("Country is required.");
        }

        Line1 = line1;
        Line2 = line2;
        City = city;
        County = county;
        PostCode = postCode;
        Country = country;
    }

    // Required by EF Core
    private Address()
    {
        Line1 = default!;
        City = default!;
        County = default!;
        PostCode = default!;
        Country = default!;
    }

    public override string ToString()
        => $"{Line1}, {City}, {PostCode}, {Country}";
}
