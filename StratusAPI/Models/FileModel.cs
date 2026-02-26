using System.ComponentModel.DataAnnotations;

namespace StratusAPI.Models
{
    public class FileModel
    {
        [Key]
        public int Id { get; set; }
        public string FileName { get; set; } // GUID-prefixed filename for storage
        public string OriginalFileName { get; set; } // Original filename for display
        public string FilePath { get; set; } // Full path in Supabase Storage (e.g., "Docs/guid_file.pdf")
        public double FileSize { get; set; } // in MB
        public string FileType { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}
