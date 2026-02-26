using System.ComponentModel.DataAnnotations;

namespace StratusAPI.Models
{
    public class ShareRequest
    {
        [Key]
        public int Id { get; set; }
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public int FileId { get; set; }
        public ShareStatus Status { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? RespondedAt { get; set; }
        
        // Navigation properties
        public User Sender { get; set; }
        public User Receiver { get; set; }
        public FileModel File { get; set; }
    }

    public enum ShareStatus
    {
        Pending = 0,
        Accepted = 1,
        Rejected = 2
    }
}