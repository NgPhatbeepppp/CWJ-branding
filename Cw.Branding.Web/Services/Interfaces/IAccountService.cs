using Cw.Branding.Web.Models.Entities;

namespace Cw.Branding.Web.Services.Interfaces
{
    public interface IAccountService
    {
        Task<AppUser> LoginAsync(string username, string password);
        string HashPassword(string password);
        bool VerifyPassword(string hashedPassword, string providedPassword);
    }
}