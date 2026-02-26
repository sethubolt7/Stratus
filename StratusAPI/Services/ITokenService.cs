namespace StratusAPI.Services
{
    public interface ITokenService
    {
        string GenerateAccessToken(string username);
        string GenerateRefreshToken();
    }
}
