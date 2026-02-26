using StratusAPI.Data;
using Supabase;
using Microsoft.EntityFrameworkCore;

namespace StratusAPI.Services
{
    public class FileService : IFileService
    {
        private Client? _client;
        private readonly IConfiguration _config;
        private readonly string _bucketName;
        private readonly StratusContext _db;

        public FileService(IConfiguration config, StratusContext db)
        {
            _config = config;
            _bucketName = config["Supabase:BucketName"] ?? "default";
            _db = db;
        }

        private Client GetClient()
        {
            if (_client == null)
            {
                var url = _config["Supabase:Url"] ?? throw new ArgumentNullException("Supabase:Url not configured");
                var secretKey = _config["Supabase:SecretKey"] ?? throw new ArgumentNullException("Supabase:SecretKey not configured");
                
                var options = new Supabase.SupabaseOptions
                {
                    AutoConnectRealtime = false
                };
                
                _client = new Client(url, secretKey, options);
            }
            return _client;
        }

        public async Task<(bool Success, string Message)> UploadFileAsync(IFormFile file, int userId)
        {
            var userFileCount = await _db.UserFiles.CountAsync(uf => uf.UserId == userId);

            if (userFileCount >= 20) return (false, "File limit reached (20 files max)");

            var userTotalSize = await _db.UserFiles
                .Where(uf => uf.UserId == userId)
                .SumAsync(uf => uf.File.FileSize);
            var fileSizeMB = file.Length / (1024.0 * 1024.0);
            if (userTotalSize + fileSizeMB > 50) return (false, "Storage limit reached (50MB max)");

            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = fileName;

            try
            {
                using var stream = file.OpenReadStream();
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                var fileBytes = memoryStream.ToArray();

                await GetClient().Storage
                    .From(_bucketName)
                    .Upload(fileBytes, filePath);

                var fileModel = new Models.FileModel()
                {
                    FileName = fileName,
                    OriginalFileName = file.FileName,
                    FilePath = filePath,
                    FileSize = fileSizeMB,
                    FileType = file.ContentType,
                    UploadedAt = DateTime.UtcNow
                };
                
                await _db.Files.AddAsync(fileModel);
                await _db.SaveChangesAsync();
                
                await _db.UserFiles.AddAsync(new Models.UserFile
                {
                    UserId = userId,
                    FileId = fileModel.Id,
                    SharedAt = DateTime.UtcNow
                });
                
                await _db.SaveChangesAsync();
                return (true, "File uploaded successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Upload failed: {ex.Message}");
            }
        }

        public async Task<bool> DeleteFileAsync(int fileId, int userId)
        {
            var userFile = await _db.UserFiles.FirstOrDefaultAsync(uf => uf.FileId == fileId && uf.UserId == userId);
            if (userFile != null)
            {
                _db.UserFiles.Remove(userFile);
            }
            
            var remainingReferences = await _db.UserFiles.CountAsync(uf => uf.FileId == fileId && uf.Id != userFile.Id);
            
            if (remainingReferences == 0)
            {
                var file = await _db.Files.FirstOrDefaultAsync(f => f.Id == fileId);
                if (file != null)
                {
                    await GetClient().Storage
                        .From(_bucketName)
                        .Remove(file.FilePath);
                    
                    _db.Files.Remove(file);
                }
            }
            
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<(bool Success, string Message)> ShareFileAsync(int senderId, int fileId, string email)
        {
            var receiver = await _db.Users.FirstOrDefaultAsync(u => u.Username == email);
            if (receiver == null)
            {
                return (false, "User not found");
            }

            var file = await _db.Files.FirstOrDefaultAsync(f => f.Id == fileId);
            if (file == null)
            {
                return (false, "File not found");
            }

            var existingRequest = await _db.ShareRequests.FirstOrDefaultAsync(sr => sr.SenderId == senderId && sr.ReceiverId == receiver.Id && sr.FileId == fileId && sr.Status == Models.ShareStatus.Pending);
            if (existingRequest != null)
            {
                return (false, "Share request already sent");
            }

            await _db.ShareRequests.AddAsync(new Models.ShareRequest
            {
                SenderId = senderId,
                ReceiverId = receiver.Id,
                FileId = fileId,
                Status = Models.ShareStatus.Pending,
                RequestedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
            return (true, "Share request sent successfully");
        }

        public async Task<List<Models.FileModel>> GetUserFilesAsync(int userId)
        {
            return await _db.UserFiles
                .Where(uf => uf.UserId == userId)
                .Select(uf => uf.File)
                .ToListAsync();
        }

        public async Task<List<Models.ShareRequest>> GetPendingShareRequestsAsync(int userId)
        {
            return await _db.ShareRequests
                .Include(sr => sr.Sender)
                .Include(sr => sr.File)
                .Where(sr => sr.ReceiverId == userId && sr.Status == Models.ShareStatus.Pending)
                .ToListAsync();
        }

        public async Task<(bool Success, string Message)> RespondToShareRequestAsync(int requestId, bool accept)
        {
            var request = await _db.ShareRequests.FirstOrDefaultAsync(sr => sr.Id == requestId);
            if (request == null)
            {
                return (false, "Share request not found");
            }

            if (request.Status != Models.ShareStatus.Pending)
            {
                return (false, "Share request already responded to");
            }

            request.Status = accept ? Models.ShareStatus.Accepted : Models.ShareStatus.Rejected;
            request.RespondedAt = DateTime.UtcNow;

            if (accept)
            {
                await _db.UserFiles.AddAsync(new Models.UserFile
                {
                    UserId = request.ReceiverId,
                    FileId = request.FileId,
                    SharedAt = DateTime.UtcNow
                });
            }

            await _db.SaveChangesAsync();
            return (true, accept ? "Share request accepted" : "Share request rejected");
        }

        public async Task<string?> GetSignedUrlAsync(int fileId, int userId)
        {
            var hasAccess = await _db.UserFiles.AnyAsync(uf => uf.FileId == fileId && uf.UserId == userId);
            if (!hasAccess) return null;

            var file = await _db.Files.FirstOrDefaultAsync(f => f.Id == fileId);
            if (file == null) return null;

            try
            {
                var signedUrl = await GetClient().Storage
                    .From(_bucketName)
                    .CreateSignedUrl(file.FilePath, 604800);
                    
                return signedUrl?.TrimEnd('?');
            }
            catch
            {
                return null;
            }
        }
    }
}