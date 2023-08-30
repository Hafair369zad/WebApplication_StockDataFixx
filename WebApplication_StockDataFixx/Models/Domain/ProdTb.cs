using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using WebApplication_StockDataFixx.Models.Domain;

namespace WebApplication_StockDataFixx.Models.Domain;

[Table("PROD_TB")]
public partial class ProdTb
{
    [Key]
    [Column("PROD_ID")]
    [StringLength(15)]
    public string ProdId { get; set; } = null!;

    [Column("CODE_ID")]
    [StringLength(10)]
    public string CodeId { get; set; } = null!;

    [Column("PLANT_ID")]
    [StringLength(10)]
    public string PlantId { get; set; } = null!;

    [Column("SLOC_DESC")]
    [StringLength(50)]
    public string SlocDesc { get; set; } = null!;

    [InverseProperty("Prod")]
    public virtual ICollection<AccessTb> AccessTbs { get; set; } = new List<AccessTb>();
}
