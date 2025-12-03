using System.ComponentModel.DataAnnotations;

namespace Cw.Branding.Web.Models.ViewModels
{
    public class ContactFormViewModel
    {
        [Required(ErrorMessage = "Please enter your name")]
        [Display(Name = "YOUR NAME")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Please enter your email")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [Display(Name = "YOUR EMAIL")]
        public string Email { get; set; }

        [Display(Name = "SELECT A PRODUCT")]
        public string SelectedProduct { get; set; }

        [Required(ErrorMessage = "Please tell us how we can help")]
        [Display(Name = "HOW CAN WE HELP YOU?")]
        public string Message { get; set; }
    }
}