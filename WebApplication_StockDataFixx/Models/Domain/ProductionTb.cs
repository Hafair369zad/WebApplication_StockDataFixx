using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication_StockDataFixx.Models.Domain;

[Table("PRODUCTION_TB")]
public partial class ProductionTb
{
    [Key]
    [Column("PROD_ID")]
    [StringLength(15)]
    public string ProdId { get; set; } = null!;

    [Column("SLOC")]
    public string Sloc { get; set; } = null!;

    [Column("PLANT_ID")]
    [StringLength(4)]
    public string PlantId { get; set; } = null!;

    [Column("SLOC_DESC")]
    public string SlocDesc { get; set; } = null!;

    [InverseProperty("Prod")]
    public virtual ICollection<AccessTb> AccessTbs { get; set; } = new List<AccessTb>();

    [InverseProperty("Prod")]
    public virtual ICollection<ProductionItem> ProductionItems { get; set; } = new List<ProductionItem>();
}
