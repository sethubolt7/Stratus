using System.ComponentModel.DataAnnotations;

namespace StratusAPI.DTO
{
    public class SignUpDTO
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
