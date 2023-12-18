namespace URLShortner.Models.Dtos.UserDtos;

public class LoginUserResponseDto
{
    public UserDto User { get; set; }

    public string Token { get; set; }
}
