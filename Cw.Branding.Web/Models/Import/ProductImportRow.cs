namespace Cw.Branding.Web.Models.Import
{
    public class ProductImportRow
    {
        public int RowIndex { get; set; } // Dòng số mấy trong Excel

        // --- Dữ liệu thô từ Excel ---
        public string Code { get; set; } // SKU - Bắt buộc 
        public string NameVi { get; set; } // Bắt buộc [cite: 16, 45]
        public string NameEn { get; set; } // Bắt buộc [cite: 16, 45]

        // Nhóm mô tả & nội dung (Mới bổ sung)
        public string? ShortDescriptionVi { get; set; }
        public string? ShortDescriptionEn { get; set; }
        public string? DescriptionVi { get; set; }      // Chứa mã HTML từ Excel 
        public string? DescriptionEn { get; set; }      // Chứa mã HTML từ Excel 
        public string? TechnicalSpecsVi { get; set; }   // Rich Text 
        public string? TechnicalSpecsEn { get; set; }   // Rich Text 

        // Nhóm tra cứu (Tên nhập từ Excel)
        public string CategoryName { get; set; } // Bắt buộc 
        public string? BrandName { get; set; }
        public string? MachineTypeName { get; set; }

        // Nhóm cấu hình (Mới bổ sung)
        public string? IsFeaturedStr { get; set; } // Nhận "Yes" hoặc "1" từ Excel 
        public int DisplayOrder { get; set; } = 0; // Thứ tự hiển thị 

        // --- Dữ liệu sau khi xử lý (Mapped IDs & Logic) ---
        public bool IsUpdate { get; set; } // True nếu trùng SKU cũ 
        public List<string> Errors { get; set; } = new List<string>();
        public bool IsValid => !Errors.Any();

        // Foreign Keys sau khi Lookup thành công 
        public int? CategoryId { get; set; }
        public int? BrandId { get; set; }
        public int? MachineTypeId { get; set; }
        public bool IsFeatured { get; set; } // Kết quả parse từ IsFeaturedStr
    }
}