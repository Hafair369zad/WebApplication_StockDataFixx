using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication_StockDataFixx.Models.Domain
{
    public class ManagementItem
    {
        public string ItemType { get; set; } = null!;
        public int ItemId { get; set; }
        public string Plant { get; set; } = null!;
        public string Sloc { get; set; } = null!;
        public string Month { get; set; } = null!;
        public string SerialNo { get; set; } = null!;
        public string TagNo { get; set; } = null!;
        public string Material { get; set; } = null!;
        public string MaterialDesc { get; set; } = null!;
        public int ActualQty { get; set; }
        public string QualInsp { get; set; } = null!;
        public string Blocked { get; set; } = null!;
        public string Unit { get; set; } = null!;
        public string StockType { get; set; } = null!;
        public string VendorCode { get; set; } = null!;
        public string VendorName { get; set; } = null!;
        public string IssuePlanner { get; set; } = null!;
        public string IsVmi { get; set; } = null!;
    }
}
