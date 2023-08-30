using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication_StockDataFixx.Models.Domain;

[Table("SECURITY_TB")]
public partial class SecurityTb
{
    [Key]
    [Column("LEVEL_ID")]
    public int LevelId { get; set; }

    [Column("LEVEL_DESC")]
    public string LevelDesc { get; set; } = null!;

    [InverseProperty("Level")]
    public virtual ICollection<UserTb> UserTbs { get; set; } = new List<UserTb>();
}
