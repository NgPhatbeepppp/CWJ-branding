using Microsoft.AspNetCore.Mvc;
using Cw.Branding.Web.Models.ViewModels;

namespace Cw.Branding.Web.Controllers
{
    [Route("{lang:regex(^(en|vi)$)}")]
    public class FbSolutionsController : Controller
    {
        [HttpGet("fb-solutions")]
        [HttpGet("giai-phap-fb")]
        public IActionResult Index(string lang = "en")
        {
            // Set ngôn ngữ cho Layout (để header/footer hiển thị đúng)
            ViewData["CurrentLang"] = lang;
            ViewData["Title"] = lang == "en" ? "F&B Solutions" : "Giải pháp F&B";

            // Hardcode nội dung tĩnh theo ngôn ngữ (vì trang này không cần CMS)
            var model = new FbSolutionViewModel();

            if (lang == "en")
            {
                model.Title = "F&B SOLUTIONS DIVISION";
                model.Description = "Explore our extensive range of food & beverage equipment solutions that bring innovation and efficiency to your culinary operations, ensuring top-quality service and satisfaction.";
                model.ButtonText = "Visit globalhoreca.com";
                model.TargetUrl = "https://globalhoreca.com"; 
            }
            else // Vietnamese
            {
                model.Title = "GIẢI PHÁP NGÀNH F&B";
                model.Description = "Khám phá các giải pháp thiết bị thực phẩm & đồ uống đa dạng, mang lại sự đổi mới và hiệu quả cho hoạt động kinh doanh ẩm thực của bạn, đảm bảo chất lượng dịch vụ hàng đầu.";
                model.ButtonText = "Truy cập globalhoreca.com";
                model.TargetUrl = "https://globalhoreca.com";
            }

            // Đường dẫn ảnh banner ( wwwroot/images/)
            model.BackgroundImagePath = "/images/fnb-hero.png";

            return View(model);
        }
    }
}