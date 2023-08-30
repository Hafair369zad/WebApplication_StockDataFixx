using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using WebApplication_StockDataFixx.Models.Domain;

namespace WebApplication_StockDataFixx.Models.Domain;

[Table("WRH_TB")]
public partial class WrhTb
{
    [Key]
    [Column("WRH_ID")]
    [StringLength(15)]
    public string WrhId { get; set; } = null!;

    [Column("CODE_ID")]
    [StringLength(10)]
    public string CodeId { get; set; } = null!;

    [Column("PLANT_ID")]
    [StringLength(10)]
    public string PlantId { get; set; } = null!;

    [Column("PLANNER_DESC")]
    [StringLength(50)]
    public string PlannerDesc { get; set; } = null!;

    [InverseProperty("Wrh")]
    public virtual ICollection<AccessTb> AccessTbs { get; set; } = new List<AccessTb>();
}
