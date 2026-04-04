namespace Cw.Branding.Web.Services.Interfaces
{
    public interface IFileService
    {
        // Trả về path của file sau khi lưu thành công
        Task<string> UploadImageAsync(IFormFile file, string folderName);
        void DeleteImage(string imagePath);
    }
}
