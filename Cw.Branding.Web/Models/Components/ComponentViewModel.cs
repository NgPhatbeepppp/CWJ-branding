using System;

namespace Cw.Branding.Web.Models.Components
{
    // Model dùng chung cho Card (Tin tức, Sản phẩm)
    public class CardItemViewModel
    {
        public string ImageUrl { get; set; } = "https://placehold.co/600x400?text=Image";
        public string Title { get; set; } = "Card Title";
        public string Description { get; set; } = "Short description goes here...";
        public string LinkUrl { get; set; } = "#";
        public string LinkText { get; set; } = "Xem chi tiết";
        public string? Date { get; set; } // Dùng cho tin tức
        public string? Category { get; set; } // Dùng cho label nhỏ (Medical / F&B)
    }

    // Model cho tiêu đề các Section (Home, Medical...)
    public class SectionHeaderViewModel
    {
        public string Title { get; set; } = "Section Title";
        public string? Subtitle { get; set; }
        public string Alignment { get; set; } = "text-center"; // "text-left" hoặc "text-center"
        public bool IsDarkBackground { get; set; } = false;
    }
}