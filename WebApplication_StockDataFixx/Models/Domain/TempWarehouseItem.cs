using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication_StockDataFixx.Models.Domain;

[Table("TEMP_WAREHOUSE_ITEM")]
public partial class TempWarehouseItem
{
    [Key]
    [Column("WAREHOUSE_ID")]
    [StringLength(225)]
    public string WarehouseId { get; set; } = null!;

    [Column("PLANT")]
    public string Plant { get; set; } = null!;

    [Column("SLOC")]
    public string Sloc { get; set; } = null!;

    [Column("MONTH")]
    public string Month { get; set; } = null!;

    [Column("SERIAL_NO")]
    public string SerialNo { get; set; } = null!;

    [Column("TAG_NO")]
    public string TagNo { get; set; } = null!;

    [Column("STOCK_TYPE")]
    public string StockType { get; set; } = null!;

    [Column("VENDOR_CODE")]
    public string VendorCode { get; set; } = null!;

    [Column("VENDOR_NAME")]
    public string VendorName { get; set; } = null!;

    [Column("MATERIAL")]
    public string Material { get; set; } = null!;

    [Column("MATERIAL_DESC")]
    public string MaterialDesc { get; set; } = null!;

    [Column("ACTUAL_QTY")]
    public int ActualQty { get; set; }

    [Column("QUAL_INSP")]
    public string QualInsp { get; set; } = null!;

    [Column("BLOCKED")]
    public string Blocked { get; set; } = null!;

    [Column("UNIT")]
    public string Unit { get; set; } = null!;

    [Column("ISSUE_PLANNER")]
    public string IssuePlanner { get; set; } = null!;

    [Column("ISVMI")]
    public string Isvmi { get; set; } = null!;

    [Column("LAST_UPLOAD", TypeName = "datetime")]
    public DateTime LastUpload { get; set; }

    [Column("LAST_INPUT", TypeName = "datetime")]
    public DateTime? LastInput { get; set; }

    [Column("DESCRIPTION")]
    public string Description { get; set; } = null!;

    [Column("WRH_ID")]
    [StringLength(15)]
    public string WrhId { get; set; } = null!;

    [Column("ACCESS_PLANT")]
    [StringLength(10)]
    public string AccessPlant { get; set; } = null!;

    [ForeignKey("AccessPlant")]
    [InverseProperty("TempWarehouseItems")]
    public virtual AccessPlantTb AccessPlantNavigation { get; set; } = null!;

    [ForeignKey("WrhId")]
    [InverseProperty("TempWarehouseItems")]
    public virtual WarehouseTb Wrh { get; set; } = null!;
}
