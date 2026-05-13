using System.Threading.Tasks;

namespace Cw.Branding.Web.Services
{
    /// <summary>
    /// Interface định nghĩa các phương thức gửi Email cho toàn hệ thống
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Gửi email bất đồng bộ
        /// </summary>
        /// <param name="subject">Tiêu đề email</param>
        /// <param name="body">Nội dung email (hỗ trợ HTML)</param>
        Task SendEmailAsync(string subject, string body);
    }
}