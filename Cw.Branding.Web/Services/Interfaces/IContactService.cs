using Cw.Branding.Web.Models.Entities;
using Cw.Branding.Web.Models.Enums;

namespace Cw.Branding.Web.Services.Interfaces
{
    public interface IContactService
    {
        // Lấy danh sách kèm phân trang/lọc (Dùng cho Admin)
        Task<IEnumerable<ContactFormEntry>> GetContactsAsync(ContactStatus? status, ContactRegion? region);

        // Lấy chi tiết và tự động đánh dấu đã đọc
        Task<ContactFormEntry?> GetDetailsAndMarkAsReadAsync(int id, string adminId);

        // Cập nhật trạng thái và ghi chú nội bộ
        Task<bool> UpdateStatusAsync(int id, ContactStatus status, string? adminNote);

        // Đếm số tin nhắn mới cho Badge thông báo trên Sidebar Admin
        Task<int> GetUnreadCountAsync();

        // Thêm vào IContactService.cs
        Task<byte[]> ExportToExcelAsync(ContactStatus? status);


    }
}