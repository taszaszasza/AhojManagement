using Infra;

namespace Service.Security;

public interface ITokenService
{
    string CreateToken(User user);
}