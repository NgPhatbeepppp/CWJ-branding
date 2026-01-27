using Microsoft.AspNetCore.Mvc;

namespace Cw.Branding.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Upload")]
    public class UploadController : Controller
    {
        private readonly IWebHostEnvironment _environment;

        public UploadController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        [HttpPost("Image")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            // 1. Validate file
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { error = "No file uploaded" });
            }

            // Chỉ cho phép các đuôi ảnh cơ bản
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest(new { error = "Invalid file type. Only images are allowed." });
            }

            try
            {
                // 2. Chuẩn bị thư mục lưu trữ: /wwwroot/uploads/news-content/
                // Dùng webRootPath để lấy đường dẫn vật lý chính xác trên server
                string webRootPath = _environment.WebRootPath;
                string uploadFolder = Path.Combine(webRootPath, "uploads", "news-content");

                // Tạo thư mục nếu chưa có
                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                // 3. Rename file (GUID + Timestamp) để tránh trùng tên
                string uniqueFileName = $"{Guid.NewGuid()}_{DateTime.Now.Ticks}{extension}";
                string filePath = Path.Combine(uploadFolder, uniqueFileName);

                // 4. Lưu file vật lý
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // 5. Trả về JSON theo format TinyMCE yêu cầu
                // Format: { "location": "/path/to/image.jpg" }
                var fileUrl = $"/uploads/news-content/{uniqueFileName}";

                return Ok(new { location = fileUrl });
            }
            catch (Exception ex)
            {
                // Log lỗi nếu cần (Serilog)
                return StatusCode(500, new { error = "Internal server error: " + ex.Message });
            }
        }
    }
}