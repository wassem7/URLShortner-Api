namespace URLShortner.Models.Dtos.SubscriptionDtos;

public class UpdateSubscriptionDto
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public int Maxurls { get; set; }

    public Decimal Price { get; set; }

    public int TotalSubsciptions { get; set; }
}
