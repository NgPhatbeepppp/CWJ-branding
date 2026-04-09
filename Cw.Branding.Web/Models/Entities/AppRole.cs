namespace Cw.Branding.Web.Models.Entities
{
    public class AppRole
    {
        public int Id { get; set; }
        public string Name { get; set; } // "Admin" hoặc "User"
        public string Description { get; set; }

        // Navigation property
        public virtual ICollection<AppUser> Users { get; set; }
    }
}