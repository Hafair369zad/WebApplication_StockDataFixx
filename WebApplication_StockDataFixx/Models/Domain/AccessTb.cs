using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication_StockDataFixx.Models.Domain;

[Table("ACCESS_TB")]
public partial class AccessTb
{
    [Key]
    [Column("ACCOUNT_ID")]
    [StringLength(50)]
    public string AccountId { get; set; } = null!;

    [Column("USER_ID")]
    [StringLength(30)]
    public string UserId { get; set; } = null!;

    [Column("PROD_ID")]
    [StringLength(15)]
    public string? ProdId { get; set; }

    [Column("WRH_ID")]
    [StringLength(15)]
    public string? WrhId { get; set; }

    [Column("SLOC")]
    public string? Sloc { get; set; }

    [Column("ISSUE_PLANNER")]
    public string? IssuePlanner { get; set; }

    [Column("PLANT_ID")]
    [StringLength(4)]
    public string PlantId { get; set; } = null!;

    [ForeignKey("ProdId")]
    [InverseProperty("AccessTbs")]
    public virtual ProductionTb? Prod { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("AccessTbs")]
    public virtual UserTb User { get; set; } = null!;

    [ForeignKey("WrhId")]
    [InverseProperty("AccessTbs")]
    public virtual WarehouseTb? Wrh { get; set; }
}
