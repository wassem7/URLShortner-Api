using URLShortner.Models.DomainModels;

namespace URLShortner.Repository;

public interface IUrlRepository
{
    Task CreateShortUrl(UrlManagement shortUrl);
}
