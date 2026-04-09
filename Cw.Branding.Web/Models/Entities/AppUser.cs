using System.ComponentModel.DataAnnotations;

namespace Cw.Branding.Web.Models.Entities
{
    public class AppUser
    {
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string Username { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public string FullName { get; set; }
        public string Email { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Foreign Key
        public int RoleId { get; set; }
        public virtual AppRole Role { get; set; }
    }
}