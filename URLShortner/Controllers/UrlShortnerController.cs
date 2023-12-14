using Microsoft.AspNetCore.Mvc;
using URLShortner.Data;
using URLShortner.Models;
using URLShortner.Models.Dtos;

namespace URLShortner.Controllers;

[ApiController]
[Route("api/shortenurl")]
public class UrlShortnerController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public UrlShortnerController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpPost("CreateShortUrl")]
    public async Task<IActionResult> CreateShortUrl([FromBody] UrlDto requestDto)
    {
        if (!Uri.TryCreate(requestDto.Url, UriKind.Absolute, out var urResult))
        {
            return BadRequest("Invalid Url");
        }

        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        var random = new Random();

        var randomStr = new string(
            Enumerable.Repeat(chars, 6).Select(x => x[random.Next(x.Length)]).ToArray()
        );

        var sUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/{randomStr}";
        var shortUrl = new UrlManagement() { LongUrl = requestDto.Url, Shorturl = randomStr };

        await _db.Urls.AddAsync(shortUrl);
        await _db.SaveChangesAsync();

        return Ok(sUrl);
    }
}
