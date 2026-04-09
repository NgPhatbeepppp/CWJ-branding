using Cw.Branding.Web.Data;
using Cw.Branding.Web.Models.Entities;
using Cw.Branding.Web.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Cw.Branding.Web.Services.Implementations
{
    public class AccountService : IAccountService
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher<AppUser> _passwordHasher;

        public AccountService(AppDbContext context, IPasswordHasher<AppUser> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task<AppUser> LoginAsync(string username, string password)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

            if (user != null && VerifyPassword(user.PasswordHash, password))
            {
                return user;
            }
            return null;
        }

        public string HashPassword(string password) => _passwordHasher.HashPassword(null, password);

        public bool VerifyPassword(string hashedPassword, string providedPassword)
        {
            var result = _passwordHasher.VerifyHashedPassword(null, hashedPassword, providedPassword);
            return result == PasswordVerificationResult.Success;
        }
    }
}