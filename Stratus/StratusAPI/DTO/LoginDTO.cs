using System.ComponentModel.DataAnnotations;

namespace StratusAPI.DTO
{
    public class LoginDTO
    {
        [Required] 
        public string Username { get; set; }

        [Required]
        required public string Password { get; set; }
    }
}
