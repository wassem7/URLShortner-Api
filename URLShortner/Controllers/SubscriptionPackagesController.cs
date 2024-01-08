using System.Net;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using URLShortner.Data;
using URLShortner.Models;
using URLShortner.Models.Dtos.SubscriptionDtos;

namespace URLShortner.Controllers;

[ApiController]
[Route("api/packages")]
public class SubscriptionPackagesController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private ApiResponse _apiResponse;

    public SubscriptionPackagesController(ApplicationDbContext db)
    {
        _db = db;
        _apiResponse = new ApiResponse();
    }

    /// <summary>
    /// Create subscription Package
    /// </summary>



    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [HttpPost("createsubscriptionpackage")]
    public async Task<IActionResult> CreateSubscriptionPackage(
        CreateSubscriptionPackageDto createSubscriptionPackageDto
    )
    {
        try
        {
            if (createSubscriptionPackageDto is null)
            {
                _apiResponse.HttpStatusCode = HttpStatusCode.BadRequest;
                _apiResponse.ErrorMessages = new List<string>() { "Invalid subscription package" };
                return BadRequest(_apiResponse);
            }

            var subscriptionPackage = await _db.SubscriptionPackages.AddAsync(
                new SubscriptionPackage()
                {
                    Name = createSubscriptionPackageDto.Name,
                    Price = createSubscriptionPackageDto.Price,
                    MaxUrls = createSubscriptionPackageDto.Maxurls,
                    TotalSubsciptions = 0
                }
            );

            _db.SaveChangesAsync();
            _apiResponse.HttpStatusCode = HttpStatusCode.OK;
            _apiResponse.SuccessMessage = "Subscription Package created";
            _apiResponse.IsSuccess = true;
            _apiResponse.Result = subscriptionPackage.Entity;

            return Ok(_apiResponse);
        }
        catch (Exception ex)
        {
            _apiResponse.ErrorMessages = new List<string>() { ex.Message.ToString(), };

            _apiResponse.HttpStatusCode = HttpStatusCode.BadRequest;

            return BadRequest(_apiResponse);
        }
    }

    /// <summary>
    /// Get all subscription packages
    /// </summary>

    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [HttpGet(Name = "getallpackages")]
    public async Task<IActionResult> GetAllSubscriptionPackages()
    {
        try
        {
            var packages = await _db.SubscriptionPackages.ToListAsync();

            _apiResponse.IsSuccess = true;
            _apiResponse.HttpStatusCode = HttpStatusCode.OK;
            _apiResponse.SuccessMessage = "All subscription Packages retrieved";
            _apiResponse.Result = packages;

            return Ok(_apiResponse);
        }
        catch (Exception ex)
        {
            _apiResponse.ErrorMessages = new List<string>() { ex.Message.ToString(), };

            _apiResponse.HttpStatusCode = HttpStatusCode.BadRequest;

            return BadRequest(_apiResponse);
        }
    }

    /// <summary>
    /// Update sucbscription packages
    /// </summary>

    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [HttpPatch("{id:guid}", Name = "updatesubscriptionpackage")]
    public async Task<IActionResult> UpdatePackage(
        [FromBody] JsonPatchDocument<UpdateSubscriptionDto> patchDocument,
        Guid id
    )
    {
        if (patchDocument is null)
        {
            _apiResponse.HttpStatusCode = HttpStatusCode.BadRequest;
            _apiResponse.ErrorMessages = new List<string>() { "Invalid patch document" };
            return BadRequest(_apiResponse);
        }

        var packageExists = await _db.SubscriptionPackages
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

        if (packageExists is null)
        {
            _apiResponse.HttpStatusCode = HttpStatusCode.NotFound;
            _apiResponse.ErrorMessages = new List<string>() { "Subscription Package not found" };
            return BadRequest(_apiResponse);
        }

        var subscriptionPackage = new UpdateSubscriptionDto()
        {
            Name = packageExists.Name,
            Price = packageExists.Price,
            Maxurls = packageExists.MaxUrls,
            TotalSubsciptions = packageExists.MaxUrls,
            Id = packageExists.Id
        };

        patchDocument.ApplyTo(subscriptionPackage, ModelState);

        if (!ModelState.IsValid)
        {
            _apiResponse.HttpStatusCode = HttpStatusCode.NotFound;
            _apiResponse.ErrorMessages = new List<string>() { "Invalid Patch Document" };
            return BadRequest(_apiResponse);
        }

        var updatedPackage = new SubscriptionPackage()
        {
            Name = subscriptionPackage.Name,
            Price = subscriptionPackage.Price,
            MaxUrls = subscriptionPackage.Maxurls,
            TotalSubsciptions = subscriptionPackage.TotalSubsciptions,
            Id = subscriptionPackage.Id
        };

        _db.Update(updatedPackage);
        await _db.SaveChangesAsync();

        _apiResponse.IsSuccess = true;
        _apiResponse.HttpStatusCode = HttpStatusCode.OK;
        _apiResponse.SuccessMessage = "Subscription Package updated";
        _apiResponse.Result = updatedPackage;

        return Ok(_apiResponse);
    }
}
