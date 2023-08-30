using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication_StockDataFixx.Models.Domain;

[Table("PLANT_TB")]
public partial class PlantTb
{
    [Key]
    [Column("PLANT_ID")]
    [StringLength(4)]
    public string PlantId { get; set; } = null!;

    [Column("PLANT_DESC")]
    public string PlantDesc { get; set; } = null!;

    [InverseProperty("Plant")]
    public virtual ICollection<UserTb> UserTbs { get; set; } = new List<UserTb>();
}
