using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication_StockDataFixx.Models.Domain;

[Table("WAREHOUSE_TB")]
public partial class WarehouseTb
{
    [Key]
    [Column("WRH_ID")]
    [StringLength(15)]
    public string WrhId { get; set; } = null!;

    [Column("ISSUE_PLANNER")]
    public string IssuePlanner { get; set; } = null!;

    [Column("PLANT_ID")]
    [StringLength(4)]
    public string PlantId { get; set; } = null!;

    [Column("PLANNER_DESC")]
    public string PlannerDesc { get; set; } = null!;

    [InverseProperty("Wrh")]
    public virtual ICollection<AccessTb> AccessTbs { get; set; } = new List<AccessTb>();

    [InverseProperty("Wrh")]
    public virtual ICollection<TempWarehouseItem> TempWarehouseItems { get; set; } = new List<TempWarehouseItem>();

    [InverseProperty("Wrh")]
    public virtual ICollection<WarehouseItem> WarehouseItems { get; set; } = new List<WarehouseItem>();
}
