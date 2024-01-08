using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using URLShortner.Data;
using URLShortner.Models;
using URLShortner.Models.Dtos;
using URLShortner.Models.Dtos.SubscriptionDtos;
using URLShortner.Repository;

namespace URLShortner.Controllers;

/// <summary>
///
/// </summary>
[ApiController]
[Route("api/shortenurl")]
public class UrlShortnerController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IUrlRepository _urlRepository;
    private ApiResponse _apiResponse;

    public UrlShortnerController(ApplicationDbContext db, IUrlRepository urlRepository)
    {
        _db = db;
        _urlRepository = urlRepository;
        _apiResponse = new ApiResponse();
    }

    /// <summary>
    /// Create shortened url [AUTHENTICATED]
    /// </summary>

    [HttpPost("createshorturl")]
    // [EnableRateLimiting("sliding")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
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

            var shortUrlExists = await _db.Urls.FirstOrDefaultAsync(
                s => s.LongUrl == requestDto.Url
            );

            if (shortUrlExists != null)
            {
                _apiResponse.IsSuccess = true;
                _apiResponse.HttpStatusCode = HttpStatusCode.OK;
                _apiResponse.SuccessMessage = "Short Url created";
                _apiResponse.Result = shortUrlExists.Shorturl;
                return Ok(_apiResponse);
            }
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
            _apiResponse.IsSuccess = true;
            _apiResponse.HttpStatusCode = HttpStatusCode.OK;
            _apiResponse.SuccessMessage = "Short Url created";
            _apiResponse.Result = sUrl;
            return Ok(_apiResponse);
        }
        catch (Exception ex)
        {
            _apiResponse.HttpStatusCode = HttpStatusCode.BadRequest;
            _apiResponse.ErrorMessages = new List<string>() { ex.Message.ToString() };
            return BadRequest(_apiResponse);
        }
    }

    /// <summary>
    /// Get all user's shortened urls [AUTHENTICATED]
    /// </summary>
    [HttpGet(Name = "getallusershortenurls")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserShortenUrls()
    {
        try
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var userId = identity.Claims.FirstOrDefault(c => c.Type == "Id").Value;
            var exp = identity.Claims.FirstOrDefault(c => c.Type == "exp").Value;

            var time = long.Parse(exp);
            var expirationDateTime = DateTimeOffset.FromUnixTimeSeconds(time).UtcDateTime;
            if (expirationDateTime < DateTime.Now)
            {
                _apiResponse.ErrorMessages = new List<string>()
                {
                    "Session Token Expired",
                    expirationDateTime.ToString()
                };
                _apiResponse.HttpStatusCode = HttpStatusCode.Forbidden;

                return BadRequest(_apiResponse);
            }
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
                _apiResponse.ErrorMessages = new List<string>() { "User has no converted Urls" };
                _apiResponse.HttpStatusCode = HttpStatusCode.NotFound;

                return BadRequest(_apiResponse);
            }

            _apiResponse.HttpStatusCode = HttpStatusCode.OK;
            _apiResponse.Result = user;
            _apiResponse.SuccessMessage = "User urls retrieved";
            _apiResponse.IsSuccess = true;
            return Ok(_apiResponse);
        }
        catch (Exception ex)
        {
            _apiResponse.HttpStatusCode = HttpStatusCode.BadRequest;
            _apiResponse.ErrorMessages = new List<string>() { ex.Message.ToString() };
            return BadRequest(_apiResponse);
        }
    }


    
}
