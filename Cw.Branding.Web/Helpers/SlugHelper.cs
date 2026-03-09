using System.Text.RegularExpressions;
using System.Text;

namespace Cw.Branding.Web.Helpers;

public static class SlugHelper
{
    public static string GenerateSlug(string phrase)
    {
        string str = phrase.ToLower();
        str = Regex.Replace(str, @"[áàảãạâấầẩẫậăắằẳẵặ]", "a");
        str = Regex.Replace(str, @"[éèẻẽẹêếềểễệ]", "e");
        str = Regex.Replace(str, @"[íìỉĩị]", "i");
        str = Regex.Replace(str, @"[óòỏõọôốồổỗộơớờởỡợ]", "o");
        str = Regex.Replace(str, @"[úùủũụưứừửữự]", "u");
        str = Regex.Replace(str, @"[ýỳỷỹỵ]", "y");
        str = Regex.Replace(str, @"d", "d");
        str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
        str = Regex.Replace(str, @"\s+", " ").Trim();
        str = str.Substring(0, str.Length <= 45 ? str.Length : 45).Trim();
        str = Regex.Replace(str, @"\s", "-");
        return str;
    }
}