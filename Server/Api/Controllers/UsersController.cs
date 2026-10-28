using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service;

namespace Api.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class UsersController(UserService service) : ControllerBase
{
    [AllowAnonymous] //everyone can call it
    [HttpPost(nameof(Login))]
    public Dtos.LoginResponseDto Login(Dtos.LoginRequestDto dto)
    {
        return service.Login(dto);
    }

    [Authorize(Roles = "Admin,Superuser")]
    [HttpPost(nameof(CreateUser))]
    public Dtos.UserResponseDto CreateUser(Dtos.CreateUserDto dto)
    {
        var currentUserRole = User.FindFirstValue(ClaimTypes.Role) ?? "User";
        return service.CreateUser(dto, currentUserRole);
    }
    
    [Authorize] // only logged in users can call it, but no matter their role
    [HttpGet(nameof(GetUsers))]
    public List<Dtos.UserResponseDto> GetUsers()
    {
        var currentUserRole = User.FindFirstValue(ClaimTypes.Role) ?? "User";
        
        return service.GetUsers(currentUserRole); // passing the role here
    }
    
    [Authorize]
    [HttpGet(nameof(GetUserById))]
    public Dtos.UserResponseDto GetUserById(string id)
    {
        var currentUserRole = User.FindFirstValue(ClaimTypes.Role) ?? "User";

        return service.GetUserById(id, currentUserRole);
    }

    [Authorize(Roles = "Admin,Superuser")]
    [HttpDelete(nameof(DeleteUser))]
    public void DeleteUser(string id)
    {
        var currentUserRole = User.FindFirstValue(ClaimTypes.Role) ?? "User";
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        
        service.DeleteUser(id, currentUserRole, currentUserId);
    }

    [Authorize(Roles = "Admin,Superuser")]
    [HttpPut(nameof(UpdateUser))]
    public Dtos.UserResponseDto UpdateUser(string userId, Dtos.UpdateUserDto dto)
    {
        var currentUserRole = User.FindFirstValue(ClaimTypes.Role) ?? "User";
        return service.UpdateUser(userId, dto, currentUserRole);
    }

}