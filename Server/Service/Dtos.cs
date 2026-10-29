namespace Service;

public class Dtos
{
    public record UpdateUserDto
    (
        string FirstName,
         string LastName,
         string? Role
    );
    
    public record CreateUserDto
        (
            string FirstName,
            string LastName,
            string Password,
            string Role
    );
    
    public record LoginRequestDto
    (
        string Login,
        string Password
    );

    public record LoginResponseDto(
        string Token,
        UserResponseDto  User);
    
    public record UserResponseDto
    (
        string UserId,
        string FirstName,
        string LastName,
        string Login,
        string Role
    );

    public record ChangePasswordRequestDto(
        string NewPassword
    );
}