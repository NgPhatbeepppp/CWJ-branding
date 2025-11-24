namespace Cw.Branding.Web.Models.Entities;

public class ContactFormEntry
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;
    public string? Company { get; set; }
    public string Email { get; set; } = null!;
    public string? Phone { get; set; }

    public string Message { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsHandled { get; set; } = false;
    public DateTime? HandledAt { get; set; }
}
