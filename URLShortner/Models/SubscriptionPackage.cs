namespace URLShortner.Models;

public class SubscriptionPackage
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public int MaxUrls { get; set; }

    public Decimal Price { get; set; }

    public int TotalSubsciptions { get; set; }
}
