using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;

namespace Service.Security;

public class Argon2PasswordHasher : IPasswordHasher
{
    public string HashPassword(string password, string salt)
    {

        //Argon2 is super heavy and we didn't dispose before. Using "using" handles disposing automatically.
        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password));

        argon2.Salt = Convert.FromBase64String(salt);
        argon2.MemorySize = 64 * 1024;
        argon2.Iterations = 3;
        argon2.DegreeOfParallelism = 4;

        string hash = Convert.ToBase64String(argon2.GetBytes(32));

        return $"{hash}.{salt}";
    }

    public string HashAndSaltPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);

        return HashPassword(password, Convert.ToBase64String(salt));
    }

    public bool VerifyHashedPassword(string password, string hashedPassword)
    {
        string[] parts = hashedPassword.Split('.');

        string hash = parts[0];
        string salt = parts[1];

        string newHashedPassword = HashPassword(password, salt);


        // == compares strings char by char and returns false once it approaches a char that doesnt match.
        // which is vulnerable to timing attacks - you can figure out how many chars are correct by how long it takes to
        //return false. That's why I changed it to constant-time protection.

        byte[] expectedBytes = Encoding.UTF8.GetBytes(hashedPassword);
        byte[] actualBytes = Encoding.UTF8.GetBytes(newHashedPassword);

        return CryptographicOperations.FixedTimeEquals(actualBytes, expectedBytes);
    }
}