using System.ComponentModel.DataAnnotations;

namespace StratusAPI.Models
{
    public class UserFile
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public int FileId { get; set; }
        public DateTime SharedAt { get; set; }
        
        // Navigation properties
        public User User { get; set; }
        public FileModel File { get; set; }
    }
}