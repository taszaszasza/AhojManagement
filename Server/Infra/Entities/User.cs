using System.Security.Cryptography.X509Certificates;
using LinqToDB.Mapping;
namespace Infra;


public class User
{
    [PrimaryKey]
    public string UserId { get; set; } = Guid.NewGuid().ToString();

    [Column(CanBeNull = false)] 
    public string FirstName { get; set; } = string.Empty;
    
    [Column(CanBeNull = false)] 
    public string LastName { get; set; } = string.Empty;
    
    [Column(CanBeNull = false)] 
    public string Login { get; set; } = string.Empty;
    
    [Column(CanBeNull = false)] 
    public string PasswordHash { get; set; } = string.Empty;

    [Column(CanBeNull = false)] 
    public string Role { get; set; } = Service.Enums.Role.User.ToString();

}