using StratusAPI.DTO;
using StratusAPI.Models;

namespace StratusAPI.Services
{
    public interface IAuthService
    {
        Task<(bool Success, string Message)> SignUpAsync(SignUpDTO req);
        Task<(bool Success,string? AccessToken, string? RefreshToken, int UserId, string Message)> LoginAsync(LoginDTO req);
        Task<(bool Success, string? AccessToken, string? RefreshToken, string Message)> RefreshAsync(string refreshToken);
    }
}
