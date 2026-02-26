using Microsoft.AspNetCore.Mvc;
using StratusAPI.Services;
using StratusAPI.DTO;

namespace StratusAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly IFileService _fileService;

        public FileController(IFileService fileService)
        {
            _fileService = fileService;
        }

        [HttpPost]
        [Route("upload")]
        public async Task<IActionResult> Upload(IFormFile file, [FromForm] int userId)
        {
            if (file == null || file.Length == 0) return BadRequest("No file uploaded.");
            if (file.Length > 10 * 1024 * 1024) return BadRequest("File size exceeds 10MB limit.");

            var result = await _fileService.UploadFileAsync(file, userId);

            if (!result.Success) return BadRequest(result.Message);

            return Ok(new { message = result.Message });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFile(int id, [FromQuery] int userId)
        {
            var result = await _fileService.DeleteFileAsync(id, userId);
            return Ok(new { success = result });
        }

        [HttpPost("share")]
        public async Task<IActionResult> ShareFile([FromBody] ShareRequestDTO request)
        {
            var result = await _fileService.ShareFileAsync(request.SenderId, request.FileId, request.Email);
            return Ok(new { success = result.Success, message = result.Message });
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserFiles(int userId)
        {
            var files = await _fileService.GetUserFilesAsync(userId);
            return Ok(files);
        }

        [HttpGet("requests/{userId}")]
        public async Task<IActionResult> GetPendingShareRequests(int userId)
        {
            var requests = await _fileService.GetPendingShareRequestsAsync(userId);
            return Ok(requests);
        }

        [HttpPost("respond")]
        public async Task<IActionResult> RespondToShareRequest([FromBody] RespondToShareRequestDTO request)
        {
            var result = await _fileService.RespondToShareRequestAsync(request.RequestId, request.Accept);
            return Ok(new { success = result.Success, message = result.Message });
        }

        [HttpGet("signed-url/{fileId}")]
        public async Task<IActionResult> GetSignedUrl(int fileId, [FromQuery] int userId)
        {
            var signedUrl = await _fileService.GetSignedUrlAsync(fileId, userId);
            if (signedUrl == null) return NotFound("File not found or access denied");
            
            return Ok(new { url = signedUrl });
        }
    }
}
