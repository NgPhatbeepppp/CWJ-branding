namespace Cw.Branding.Web.Models.Import
{
    public class ProductImportResult
    {
        public List<ProductImportRow> Rows { get; set; } = new List<ProductImportRow>();
        public int TotalRows => Rows.Count;
        public int ValidRows => Rows.Count(r => r.IsValid);
        public int ErrorRows => Rows.Count(r => !r.IsValid);
        public int NewItems => Rows.Count(r => r.IsValid && !r.IsUpdate);
        public int UpdatedItems => Rows.Count(r => r.IsValid && r.IsUpdate);
        public bool CanCommit => ErrorRows == 0 && TotalRows > 0; // Nguyên tắc Atomic 
    }
}