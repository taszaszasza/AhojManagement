using System.ComponentModel.DataAnnotations;
using Infra;
using LinqToDB;
using Service.Security;

namespace Service;



public class UserService(MyDatabaseConnection db, IPasswordHasher passwordHasher, ITokenService tokenService)
{
    public string GenerateLogin(string firstName, string lastName)
    { 
        string firstClean = firstName.ToLower().Trim();
        string lastClean = lastName.ToLower().Trim();
        
       char first = firstClean[0];
       
       char last1 = lastClean[0];
       char last2 = lastClean[1];
       char last3;
       if (lastClean.Length > 2)
       {
           last3 = lastClean[2];
       }
       else
       {
           last3 = 'x'; // if the last name is only 2 letters long, it adds x instead of the third letter
       }

       int number = Random.Shared.Next(1, 100);
       string login = $"{first}{last1}{last2}{last3}{number:D2}"; // d2 adds a zero in one digit nrs
       return login;
       
    }

    public Dtos.LoginResponseDto Login(Dtos.LoginRequestDto loginRequestDto)
    {
        if (string.IsNullOrWhiteSpace(loginRequestDto.Login) || string.IsNullOrWhiteSpace(loginRequestDto.Password)) throw new ValidationException("Username or passwsord is empty");
        var trimmed = loginRequestDto.Login.ToLower().Trim();
        
        var user = db.Users.FirstOrDefault(u => u.Login.ToLower().Trim() == trimmed);
        
        if( user == null) throw new UnauthorizedAccessException("Username or passwsord is incorrect");

        bool isPasswordCorrect = passwordHasher.VerifyHashedPassword(loginRequestDto.Password, user.PasswordHash);

        if (!isPasswordCorrect) throw new UnauthorizedAccessException("Username or password incorrect");

        return new Dtos.LoginResponseDto(
            tokenService.CreateToken(user),
            new Dtos.UserResponseDto(
                user.UserId,
                user.FirstName,
                user.LastName,
                user.Login,
                user.Role));

    }

    public Dtos.UserResponseDto CreateUser(Dtos.CreateUserDto userDto, string userRole)
    {
        if (string.IsNullOrWhiteSpace(userDto.FirstName) || string.IsNullOrWhiteSpace(userDto.LastName) ||
            string.IsNullOrWhiteSpace(userDto.Password))
            throw new ValidationException("Empty field uhoh");
        
        string login;
        do
        {
            login = GenerateLogin(userDto.FirstName, userDto.LastName);
        } while (db.Users.Any(u => u.Login == login));

        string passwordHash = passwordHasher.HashAndSaltPassword(userDto.Password);
        
        // if a superuser is trying to create anything else than user, that's a big no-no
        if (userRole == "Superuser" && userDto.Role != "User") throw new UnauthorizedAccessException("Superusers can only create regular Users.");

        var user = new User()
        {
            UserId = Guid.NewGuid().ToString(),
            FirstName = userDto.FirstName,
            LastName = userDto.LastName,
            Login = login,
            PasswordHash = passwordHash,
            Role = userDto.Role
        };

        db.Insert(user);
        return new Dtos.UserResponseDto(user.UserId, user.FirstName, user.LastName, user.Login, user.Role);
    }

    public List<Dtos.UserResponseDto> GetUsers(string callerRole)
    {
        var query = db.Users.AsQueryable();
        
        //if the role is user or suepruser (not admin), I'm limiting the search to only users
        if(callerRole != "Admin") query = query.Where(u => u.Role == "User");
        
        return query.Select( u => new Dtos.UserResponseDto(
            u.UserId,
            u.FirstName,
            u.LastName,
            u.Login,
            u.Role)).ToList();
    }

    public Dtos.UserResponseDto GetUserById(string id, string currentRole)
    {
        if (!db.Users.Any(user => user.UserId == id)) throw new KeyNotFoundException("No user with this id");
        
        var user = db.Users.Where(u => u.UserId == id).Select(u => new Dtos.UserResponseDto(
            u.UserId,
            u.FirstName,
            u.LastName,
            u.Login,
            u.Role)).FirstOrDefault();
        
        if (currentRole != "Admin" && user.Role != "User") throw new UnauthorizedAccessException("You can only access USERS");
        
        return user;
    }

    public Dtos.UserResponseDto UpdateUser(string userId, Dtos.UpdateUserDto userDto, string currentRole)
    {
        if (!db.Users.Any(user => user.UserId == userId)) throw new KeyNotFoundException("No user with this id");
        
       

        if (string.IsNullOrWhiteSpace(userDto.FirstName) || string.IsNullOrWhiteSpace(userDto.LastName))
            throw new ValidationException("blank space...");
        
        var user = db.Users.FirstOrDefault(u => u.UserId == userId);
        
        if (currentRole == "Superuser" &&  user.Role != "User") throw new UnauthorizedAccessException("You can only modify USERS");

        user.FirstName = userDto.FirstName;
        user.LastName = userDto.LastName;
        
        if (currentRole == "Superuser" && userDto.Role!="User") throw new UnauthorizedAccessException("You are not autorized to change roles");
        user.Role =  userDto.Role;
        
        db.Update(user);
        
        return new Dtos.UserResponseDto(user.UserId, user.FirstName, user.LastName, user.Login, user.Role);

    }
    
    public Dtos.UserResponseDto ChangePassword( string targetUserId, Dtos.ChangePasswordRequestDto dto, string currentUserId, string currentRole)
    {
        if (string.IsNullOrWhiteSpace(dto.NewPassword))
            throw new ValidationException("Password cannot be empty or whitespace.");
        
        if (currentRole != "Admin" && targetUserId != currentUserId)
            throw new UnauthorizedAccessException("You can only change your own password.");

        var user = db.Users.FirstOrDefault(u => u.UserId == targetUserId)
                   ?? throw new KeyNotFoundException("No user with this id.");

        user.PasswordHash = passwordHasher.HashAndSaltPassword(dto.NewPassword);
        db.Update(user);

        return new Dtos.UserResponseDto(user.UserId, user.FirstName, user.LastName, user.Login, user.Role);
    }

    public void DeleteUser(string userId, string userRole, string currentId)
    {
       var user = db.Users.FirstOrDefault(u => u.UserId == userId);
       
       if (user == null) throw new KeyNotFoundException("No user with this id");
       if(user.UserId == currentId) throw new UnauthorizedAccessException("You cannot delete yourself");
       if(userRole == "Superuser" && user.Role != "User") throw new UnauthorizedAccessException("You can only modify USERs");
       db.Delete(user);

    }






}