using URLShortner.Models.Dtos;

namespace URLShortner.Services;

public interface IMaxUrlCacheService
{
    Task SetMaxUrls(string key, int count);

    Task<object> DecreaseUrlCount(string key);

    Task<object> GetMaxUrls(string key);
}
