namespace URLShortner.Models.Dtos.SubscriptionDtos;

public class CreateSubscriptionPackageDto
{
    public string Name { get; set; }
    public int Maxurls { get; set; }
    public Decimal Price { get; set; }
    
}
