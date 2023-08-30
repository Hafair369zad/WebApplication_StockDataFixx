using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication_StockDataFixx.Models.Domain;

[Table("ACCESS_PLANT_TB")]
public partial class AccessPlantTb
{
    [Key]
    [Column("ACCESS_PLANT")]
    [StringLength(10)]
    public string AccessPlant { get; set; } = null!;

    [Column("ACCESS_DESC")]
    public string AccessDesc { get; set; } = null!;

    [InverseProperty("AccessPlantNavigation")]
    public virtual ICollection<ProductionItem> ProductionItems { get; set; } = new List<ProductionItem>();

    [InverseProperty("AccessPlantNavigation")]
    public virtual ICollection<UserTb> UserTbs { get; set; } = new List<UserTb>();

    [InverseProperty("AccessPlantNavigation")]
    public virtual ICollection<WarehouseItem> WarehouseItems { get; set; } = new List<WarehouseItem>();
}
