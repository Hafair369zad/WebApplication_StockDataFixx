using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication_StockDataFixx.Models.Domain;

[Keyless]
[Table("coba")]
public partial class Coba
{
    [Column("PLANT_ID")]
    [StringLength(10)]
    public string PlantId { get; set; } = null!;

    [Column("PLANT_DESC")]
    [StringLength(30)]
    [Unicode(false)]
    public string PlantDesc { get; set; } = null!;
}
