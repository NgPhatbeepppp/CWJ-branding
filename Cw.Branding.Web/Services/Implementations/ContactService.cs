using Cw.Branding.Web.Data;
using Cw.Branding.Web.Models.Entities;
using Cw.Branding.Web.Models.Enums;
using Cw.Branding.Web.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel; 

namespace Cw.Branding.Web.Services.Implementations
{
    public class ContactService : IContactService
    {
        private readonly AppDbContext _context;

        public ContactService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ContactFormEntry>> GetContactsAsync(ContactStatus? status, ContactRegion? region)
        {
            var query = _context.ContactFormEntries.AsQueryable();
            if (status.HasValue) query = query.Where(x => x.ProcessingStatus == status);
            if (region.HasValue) query = query.Where(x => x.Region == region); // Lọc theo vùng
            return await query.OrderByDescending(x => x.CreatedAt).ToListAsync();
        }

        public async Task<ContactFormEntry?> GetDetailsAndMarkAsReadAsync(int id, string adminId)
        {
            var entry = await _context.ContactFormEntries.FindAsync(id);
            if (entry == null) return null;

            if (!entry.IsRead)
            {
                entry.IsRead = true;
                entry.ReadAt = DateTime.UtcNow;
                entry.ReadByAdminId = adminId;
                await _context.SaveChangesAsync();
            }

            return entry;
        }

        public async Task<bool> UpdateStatusAsync(int id, ContactStatus status, string? adminNote)
        {
            var entry = await _context.ContactFormEntries.FindAsync(id);
            if (entry == null) return false;

            entry.ProcessingStatus = status;
            if (!string.IsNullOrEmpty(adminNote)) entry.AdminNotes = adminNote;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetUnreadCountAsync()
        {
            return await _context.ContactFormEntries.CountAsync(x => !x.IsRead);
        }

        public async Task<byte[]> ExportToExcelAsync(ContactStatus? status)
        {
            var query = _context.ContactFormEntries.AsQueryable();
            if (status.HasValue) query = query.Where(x => x.ProcessingStatus == status);

            var data = await query.OrderByDescending(x => x.CreatedAt).ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Leads Report");

                // 1. Header (Thêm cột Region và Company/Phone)
                var headers = new string[] { "Date", "Name", "Company", "Email", "Phone", "Region", "Product", "Status" };
                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = worksheet.Cell(1, i + 1);
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#004F9F"); // Brand Blue
                    cell.Style.Font.FontColor = XLColor.White;
                }

                // 2. Data rows
                int row = 2;
                foreach (var item in data)
                {
                    worksheet.Cell(row, 1).Value = item.CreatedAt.ToString("dd/MM/yyyy HH:mm");
                    worksheet.Cell(row, 2).Value = item.Name;
                    worksheet.Cell(row, 3).Value = item.Company ?? "-";
                    worksheet.Cell(row, 4).Value = item.Email;
                    worksheet.Cell(row, 5).Value = item.Phone ?? "-";

                    // Chuyển Enum Region sang text cho dễ đọc trong Excel
                    worksheet.Cell(row, 6).Value = item.Region.ToString();

                    worksheet.Cell(row, 7).Value = item.SelectedProduct;
                    worksheet.Cell(row, 8).Value = item.ProcessingStatus.ToString();
                    row++;
                }

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }
    }
}