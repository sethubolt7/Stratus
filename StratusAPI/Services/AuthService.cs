using Microsoft.EntityFrameworkCore;
using StratusAPI.Data;
using StratusAPI.DTO;
using StratusAPI.Models;

namespace StratusAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly StratusContext _db;
        private readonly ITokenService _tokenService;

        public AuthService(StratusContext db, ITokenService tokenService)
        {
            _db = db;
            _tokenService = tokenService;
        }

        public async Task<(bool Success, string Message)> SignUpAsync(SignUpDTO req)
        {
            // Check if username exists
            if (await _db.Users.AnyAsync(u => u.Username == req.Username)) return (false, "Username already exists");

            var user = new User
            {
                Username = req.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password)
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return (true, "User created successfully");
        }

        public async Task<(bool Success, string? AccessToken, string? RefreshToken, int UserId, string Message)> LoginAsync(LoginDTO req)
        {
            var user = await _db.Users.SingleOrDefaultAsync(u => u.Username == req.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash)) return (false, null, null, 0, "Invalid username or password");

            var accessToken = _tokenService.GenerateAccessToken(user.Username);
            var refresh = _tokenService.GenerateRefreshToken();

            var rt = new RefreshToken
            {
                Token = refresh,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                UserId = user.Id
            };

            _db.RefreshTokens.Add(rt);
            await _db.SaveChangesAsync();
            return (true, accessToken, refresh, user.Id, "Login successful");
        }

        async Task<(bool Success, string? AccessToken, string? RefreshToken, string Message)> IAuthService.RefreshAsync(string refreshToken)
        {
            var rt = await _db.RefreshTokens.Include(r => r.User)
                         .SingleOrDefaultAsync(r => r.Token == refreshToken && !r.IsRevoked);

            if (rt == null || rt.ExpiresAt < DateTime.UtcNow) return (false, null, null, "Invalid or expired refresh token");

            // rotate: revoke old, issue new
            rt.IsRevoked = true;
            var newRefresh = _tokenService.GenerateRefreshToken();

            var newRt = new RefreshToken 
            { 
                Token = newRefresh, 
                ExpiresAt = DateTime.UtcNow.AddDays(7), 
                UserId = rt.UserId 
            };

            _db.RefreshTokens.Add(newRt);

            var newAccess = _tokenService.GenerateAccessToken(rt.User.Username);
            await _db.SaveChangesAsync();

            return (true, newAccess, newRefresh, "Token refreshed successfully");
        }

    }
}
