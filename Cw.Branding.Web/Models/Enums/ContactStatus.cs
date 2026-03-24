// File: Models/Enums/ContactStatus.cs
namespace Cw.Branding.Web.Models.Enums
{
    public enum ContactStatus
    {
        New = 0,        // Mới (Lead vừa đổ về)
        InProcess = 1,  // Đang xử lý (Admin đã tiếp nhận)
        Completed = 2,  // Đã xong (Đã liên hệ/tư vấn xong)
        Rejected = 3    // Hủy/Spam (Rác hoặc khách không nhu cầu)
    }
}