using System.ComponentModel.DataAnnotations;

namespace URLShortner.Models.Dtos.UserDtos;

public class RegisterUserRequestDto
{
    public string Username { get; set; }

    public string Password { get; set; }
}
