using URLShortner.Models;

namespace URLShortner.Repository;

public interface IUrlRepository
{
    Task CreateShortUrl(UrlManagement shortUrl);
}
