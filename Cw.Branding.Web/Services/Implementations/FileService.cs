using Cw.Branding.Web.Services.Interfaces;

public class FileService : IFileService
{
    private readonly IWebHostEnvironment _environment;
    public FileService(IWebHostEnvironment environment) => _environment = environment;

    public async Task<string> UploadImageAsync(IFormFile file, string folderName)
    {
        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", folderName);
        if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

        // Tạo tên file duy nhất để tránh trùng
        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
        var filePath = Path.Combine(uploadsFolder, fileName);

        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }

        return $"/uploads/{folderName}/{fileName}";
    }

    public void DeleteImage(string imagePath)
    {
        var fullPath = Path.Combine(_environment.WebRootPath, imagePath.TrimStart('/'));
        if (File.Exists(fullPath)) File.Delete(fullPath);
    }
}