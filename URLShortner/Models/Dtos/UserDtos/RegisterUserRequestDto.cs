using System.ComponentModel.DataAnnotations;

namespace URLShortner.Models.Dtos.UserDtos;

public class RegisterUserRequestDto
{
    public string UserName { get; set; }

    public string Password { get; set; }
}
