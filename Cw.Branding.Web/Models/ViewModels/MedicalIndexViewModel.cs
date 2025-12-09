using Cw.Branding.Web.Models.Entities;

namespace Cw.Branding.Web.Models.ViewModels
{
    public class MedicalIndexViewModel
    {
        public IEnumerable<Category> Categories { get; set; } = new List<Category>();
        // Có thể thêm field cho Hero Slider sau này
        public IEnumerable<Product> InitialProducts { get; set; } = new List<Product>();
    }
}