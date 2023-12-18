using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using URLShortner.Data;
using URLShortner.Models;
using URLShortner.Models.Dtos;
using URLShortner.Repository;

namespace URLShortner.Controllers;

[ApiController]
[Route("api/shortenurl")]
public class UrlShortnerController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IUrlRepository _urlRepository;
    private ApiResponse apiResponse;

    public UrlShortnerController(ApplicationDbContext db, IUrlRepository urlRepository)
    {
        _db = db;
        _urlRepository = urlRepository;
        apiResponse = new ApiResponse();
    }

    [HttpPost("CreateShortUrl")]
    [EnableRateLimiting("sliding")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> CreateShortUrl([FromBody] UrlDto requestDto)
    {
        try
        {
            if (!Uri.TryCreate(requestDto.Url, UriKind.Absolute, out var urResult))
            {
                return BadRequest("Invalid Url");
            }

            var identity = HttpContext.User.Identity as ClaimsIdentity;

            IEnumerable<Claim> claims = identity.Claims;

            var userId = claims.FirstOrDefault(c => c.Type == "Id").Value;

            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            var random = new Random();

            var randomStr = new string(
                Enumerable.Repeat(chars, 6).Select(x => x[random.Next(x.Length)]).ToArray()
            );

            var sUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/{randomStr}";
            var shortUrl = new UrlManagement()
            {
                LongUrl = requestDto.Url,
                Shorturl = randomStr,
                UserId = userId
            };

            await _urlRepository.CreateShortUrl(shortUrl);

            return Ok(sUrl);
        }
        catch (Exception ex)
        {
            apiResponse.HttpStatusCode = HttpStatusCode.BadRequest;
            apiResponse.ErrorMessages = new List<string>() { ex.Message.ToString() };
            return BadRequest(apiResponse);
        }
    }

    [HttpGet(Name = "GetAllUserShortenUrls")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    // [Authorize]
    public async Task<IActionResult> GetUserShortenUrls()
    {
        try
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var userId = identity.Claims.FirstOrDefault(c => c.Type == "Id").Value;
            var user = await _db.Urls
                .Where(u => u.UserId == userId)
                .Select(
                    u =>
                        new
                        {
                            u.Id,
                            u.Shorturl,
                            u.LongUrl
                        }
                )
                .ToListAsync();

            if (user == null)
            {
                apiResponse.ErrorMessages = new List<string>() { "User has no converted Urls" };
                apiResponse.HttpStatusCode = HttpStatusCode.NotFound;

                return BadRequest(apiResponse);
            }

            apiResponse.HttpStatusCode = HttpStatusCode.OK;
            apiResponse.Result = user;
            apiResponse.SuccessMessage = "User urls retrieved";
            apiResponse.IsSuccess = true;
            return Ok(apiResponse);
        }
        catch (Exception ex)
        {
            apiResponse.HttpStatusCode = HttpStatusCode.BadRequest;
            apiResponse.ErrorMessages = new List<string>() { ex.Message.ToString() };
            return BadRequest(apiResponse);
        }
    }
}
