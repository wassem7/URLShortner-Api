using StackExchange.Redis;
using URLShortner.Models.Dtos;

namespace URLShortner.Services;

public class MaxUrlsCacheService : IMaxUrlCacheService
{
    private IDatabase _cacheDb;
    TimeSpan _expiryTime = DateTimeOffset.Now.AddDays(1).DateTime.Subtract(DateTime.Now);

    public MaxUrlsCacheService()
    {
        var redis = ConnectionMultiplexer.Connect("localhost:6379");
        _cacheDb = redis.GetDatabase(4);
    }

    public async Task SetMaxUrls(string key, int maxurls)
    {
        await _cacheDb.StringSetAsync(key, maxurls - 1, _expiryTime);
    }

    public async Task<object> DecreaseUrlCount(string key)
    {
        var keyExists = _cacheDb.KeyExists(key);
        if (keyExists)
        {
            var data = await _cacheDb.StringDecrementAsync(key, 1);
            return data;
        }

        return null;
    }

    public async Task<object> GetMaxUrls(string key)
    {
        var keyExists = _cacheDb.KeyExists(key);
        if (keyExists)
        {
            return await _cacheDb.StringGetAsync(key);
        }

        return null;
    }
}
