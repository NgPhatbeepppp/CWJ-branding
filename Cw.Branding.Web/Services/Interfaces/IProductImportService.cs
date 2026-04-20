using Cw.Branding.Web.Models.Import;

namespace Cw.Branding.Web.Services.Interfaces
{
    public interface IProductImportService
    {
        // Bước "Dry Run": Đọc file, kiểm tra lỗi, chưa lưu vào DB
        Task<ProductImportResult> ValidateExcelAsync(Stream fileStream);

        // Bước "Commit": Thực hiện lưu dữ liệu vào DB (dùng Transaction)
        Task<(bool Success, string Message)> CommitImportAsync(List<ProductImportRow> validRows);
    }
}