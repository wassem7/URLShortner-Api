using System.ComponentModel.DataAnnotations;

namespace URLShortner.Models.DomainModels;

public class User
{
    [Key]
    public Guid Id { get; set; }

    public string Username { get; set; }

    public string Password { get; set; }

    public string Subscription { get; set; }

    public Guid SubscriptionId { get; set; }
}
