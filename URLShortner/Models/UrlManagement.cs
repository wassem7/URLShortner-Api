using System.ComponentModel.DataAnnotations.Schema;

namespace URLShortner.Models;

public class UrlManagement
{
    public Guid Id { get; set; }

    public string LongUrl { get; set; }

    public string Shorturl { get; set; }

    public string UserId { get; set; }
}
