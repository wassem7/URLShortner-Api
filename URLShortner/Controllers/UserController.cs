using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mime;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using URLShortner.Data;
using URLShortner.Models;
using URLShortner.Models.Dtos.UserDtos;
using URLShortner.Repository.UserRepository;
using BC = BCrypt.Net.BCrypt;

namespace URLShortner.Controllers;

[ApiController]
[Route("api/users/auth")]
public class UserController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private ApiResponse apiResponse;
    private readonly IConfiguration _configuration;
    private readonly IUserRepository _userRepository;

    public UserController(
        ApplicationDbContext db,
        IConfiguration configuration,
        IUserRepository userRepository
    )
    {
        apiResponse = new ApiResponse();
        _userRepository = userRepository;
        _db = db;
        _configuration = configuration;
    }

    /// <summary>
    /// Register user
    /// </summary>

    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [AllowAnonymous]
    [HttpPost("registeruser")]
    public async Task<ActionResult<ApiResponse>> RegisterUser(
        [FromBody] RegisterUserRequestDto registerUserRequestDto
    )
    {
        try
        {
            var userExists = await _userRepository.GetUserAsync(
                u => u.Username == registerUserRequestDto.Username
            );

            if (userExists != null)
            {
                apiResponse.IsSuccess = false;
                apiResponse.ErrorMessages = new List<string>()
                {
                    "User already exists in the system"
                };

                return BadRequest(apiResponse);
            }

            var subscription = await _db.SubscriptionPackages.FirstOrDefaultAsync(
                s => s.Name.ToLower() == "free"
            );

            var user = new User()
            {
                Username = registerUserRequestDto.Username,
                Password = BC.HashPassword(registerUserRequestDto.Password),
                Subscription = subscription.Name,
                SubscriptionId = subscription.Id
            };

            await _userRepository.AddUserAsync(user);
            await _db.SaveChangesAsync();
            apiResponse.IsSuccess = true;
            apiResponse.HttpStatusCode = HttpStatusCode.Created;
            apiResponse.Result = new RegisterUserResponseDto()
            {
                Username = user.Username,
                Subscription = user.Subscription
            };
            apiResponse.SuccessMessage = "User created successfully";
            return Ok(apiResponse);
        }
        catch (Exception ex)
        {
            apiResponse.ErrorMessages = new List<string>() { ex.Message.ToString() };
            apiResponse.HttpStatusCode = HttpStatusCode.InternalServerError;
            return BadRequest(apiResponse);
        }
    }

    /// <summary>
    /// Login user. Token expires in 24 hours
    /// </summary>
    [HttpPost("loginuser")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse>> LoginUser(
        [FromBody] LoginUserRequestDto loginUserRequestDto
    )
    {
        try
        {
            var user = await _db.Users.FirstOrDefaultAsync(
                u => u.Username == loginUserRequestDto.Username
            );

            if (user == null)
            {
                apiResponse.ErrorMessages = new List<string>() { "Invalid Credentials" };
                apiResponse.HttpStatusCode = HttpStatusCode.BadRequest;
                apiResponse.Result = new LoginUserResponseDto() { User = null, Token = "" };
                return BadRequest(apiResponse);
            }

            var isPasswordValid = BC.Verify(loginUserRequestDto.Password, user.Password);

            if (!isPasswordValid)
            {
                apiResponse.ErrorMessages = new List<string>() { "Invalid Credentials" };
                apiResponse.HttpStatusCode = HttpStatusCode.BadRequest;
                return BadRequest(apiResponse);
            }

            //GENERATE TOKEN
            var tokenHandler = new JwtSecurityTokenHandler();

            var secretKey = _configuration.GetValue<string>("ApiSettings:Secret");
            if (secretKey.IsNullOrEmpty())
            {
                apiResponse.HttpStatusCode = HttpStatusCode.BadRequest;
                apiResponse.Result = new LoginUserResponseDto() { User = null, Token = "" };
                return BadRequest(apiResponse);
            }

            var key = Encoding.ASCII.GetBytes(secretKey);
            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(
                    new Claim[]
                    {
                        new Claim("Username", user.Username),
                        new Claim("Id", user.Id.ToString())
                    }
                ),
                Expires = DateTime.UtcNow.AddDays(2),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                ),
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var loginresponsedto = new LoginUserResponseDto()
            {
                Token = tokenHandler.WriteToken(token),
                User = new UserDto() { Username = user.Username, },
            };

            apiResponse.IsSuccess = true;
            apiResponse.HttpStatusCode = HttpStatusCode.Created;
            apiResponse.Result = loginresponsedto;
            apiResponse.SuccessMessage = "Login successful";
            return Ok(apiResponse);
        }
        catch (Exception ex)
        {
            apiResponse.ErrorMessages = new List<string>() { ex.Message.ToString() };
            apiResponse.HttpStatusCode = HttpStatusCode.InternalServerError;
            return BadRequest(apiResponse);
        }
    }

    /// <summary>
    /// All users
    /// </summary>
    /// <returns></returns>
    [HttpGet("AllUsers")]
    [Produces(MediaTypeNames.Application.Json)]
    public async Task<ActionResult> GetAllUsers()
    {
        try
        {
            apiResponse.HttpStatusCode = HttpStatusCode.OK;
            apiResponse.IsSuccess = true;
            apiResponse.Result = await _db.Users.ToListAsync();
            apiResponse.SuccessMessage = "All users retrieved";
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
