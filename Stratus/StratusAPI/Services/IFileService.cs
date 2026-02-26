namespace StratusAPI.Services
{
    public interface IFileService
    {
        Task<(bool Success, string Message)> UploadFileAsync(IFormFile file, int userId);
        Task<bool> DeleteFileAsync(int fileId, int userId);
        Task<(bool Success, string Message)> ShareFileAsync(int senderId, int fileId, string email);
        Task<List<Models.FileModel>> GetUserFilesAsync(int userId);
        Task<List<Models.ShareRequest>> GetPendingShareRequestsAsync(int userId);
        Task<(bool Success, string Message)> RespondToShareRequestAsync(int requestId, bool accept);
        Task<string?> GetSignedUrlAsync(int fileId, int userId);

    }
}
