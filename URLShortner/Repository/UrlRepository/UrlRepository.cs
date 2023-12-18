using URLShortner.Data;
using URLShortner.Models;

namespace URLShortner.Repository;

public class UrlRepository : IUrlRepository
{
    private readonly ApplicationDbContext _db;

    public UrlRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task CreateShortUrl(UrlManagement shortUrl)
    {
        await _db.Urls.AddAsync(shortUrl);
        await _db.SaveChangesAsync();
    }
}
