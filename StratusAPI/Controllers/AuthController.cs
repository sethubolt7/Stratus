using Microsoft.AspNetCore.Mvc;
using StratusAPI.DTO;
using StratusAPI.Services;

namespace StratusAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        #region SignUp
        [HttpPost("signup")]
        public async Task<IActionResult> SignUp(SignUpDTO req)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            var result = await _authService.SignUpAsync(req);

            if(!result.Success) return BadRequest(new { message = result.Message });
            
            return Ok(new { message = result.Message });
        }
        #endregion SignUp


        #region Login

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO req)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _authService.LoginAsync(req);

            if (!result.Success) return Unauthorized(new { message = result.Message });

            return Ok(new
            {
                AccessToken = result.AccessToken ?? null,
                RefreshToken = result.RefreshToken ?? null,
                UserId = result.UserId,
                message = result.Message
            });
        }
        #endregion Login


        #region Refresh

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken)) return BadRequest(new { message = "Refresh token is required" });
            
            var result = await _authService.RefreshAsync(refreshToken);

            if (!result.Success) return Unauthorized(new { message = result.Message });

            var newAccess = result.AccessToken ?? null;
            var newRefresh = result.RefreshToken ?? null;

            return Ok(new { AccessToken = newAccess, RefreshToken = newRefresh });
        }

        #endregion Refresh
    }
}
