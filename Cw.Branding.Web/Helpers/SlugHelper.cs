using System.Text.RegularExpressions;
using System.Text;

namespace Cw.Branding.Web.Helpers
{
    public static class SlugHelper
    {
        public static string GenerateSlug(string phrase)
        {
            if (string.IsNullOrEmpty(phrase)) return "";

            // 1. Chuyển về chữ thường
            string str = phrase.ToLower().Trim();

            // 2. Thay thế các ký tự tiếng Việt có dấu (Bản cập nhật đầy đủ hơn)
            str = Regex.Replace(str, @"[áàảãạâấầẩẫậăắằẳẵặ]", "a");
            str = Regex.Replace(str, @"[éèẻẽẹêếềểễệ]", "e");
            str = Regex.Replace(str, @"[íìỉĩị]", "i");
            str = Regex.Replace(str, @"[óòỏõọôốồổỗộơớờởỡợ]", "o");
            str = Regex.Replace(str, @"[úùủũụưứừửữự]", "u");
            str = Regex.Replace(str, @"[ýỳỷỹỵ]", "y");
            str = Regex.Replace(str, @"[đ]", "d"); // Đã sửa lỗi 'd' -> 'd' của bản cũ

            // 3. Loại bỏ ký tự đặc biệt
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");

            // 4. Thay khoảng trắng thành dấu gạch ngang và xử lý gạch ngang thừa
            str = Regex.Replace(str, @"\s+", "-").Trim();
            str = Regex.Replace(str, @"-+", "-");

            // 5. ĐỘ DÀI (Cân nhắc): 
          
            if (str.Length > 150) str = str.Substring(0, 150).Trim('-');

            return str;
        }
    }
}