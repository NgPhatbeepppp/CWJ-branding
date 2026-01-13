using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Cw.Branding.Web.Data;
using Cw.Branding.Web.Models.ViewModels;
using Cw.Branding.Web.Models;
using Cw.Branding.Web.Models.Entities;
using System.Diagnostics;

namespace Cw.Branding.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new HomeViewModel();

            try
            {
                // Fetch Latest News
                viewModel.LatestNews = await _context.News
                    .Where(n => n.IsPublished && n.PublishedAt <= DateTime.Now)
                    .OrderByDescending(n => n.IsPublished)
                    .Take(3)
                    .ToListAsync();

              
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Homepage data");
            }

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Submit(ContactFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Save contact form entry to database
                    var contactEntry = new ContactFormEntry
                    {
                        Name = model.Name,
                        Company = null, // Not provided in the form
                        Email = model.Email,
                        Phone = null, // Not provided in the form
                        Message = model.Message,
                        CreatedAt = DateTime.UtcNow,
                        IsHandled = false
                    };

                    _context.ContactFormEntries.Add(contactEntry);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Contact form submitted successfully from {Email}", model.Email);
                    
                    // Redirect to home with success message
                    TempData["SuccessMessage"] = "Thank you for contacting us! We will get back to you soon.";
                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving contact form entry");
                    TempData["ErrorMessage"] = "There was an error submitting your form. Please try again.";
                }
            }

            // If validation fails or exception occurs, redirect back
            return RedirectToAction("Index", "Home");
        }
        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}