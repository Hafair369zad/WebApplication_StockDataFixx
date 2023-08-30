using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication_StockDataFixx.Models.Domain;

[Table("security_lvl_tb")]
public partial class SecurityLvlTb
{
    [Key]
    [Column("level_id")]
    public int LevelId { get; set; }

    [Column("level_desc")]
    [StringLength(50)]
    [Unicode(false)]
    public string LevelDesc { get; set; } = null!;

    [InverseProperty("Level")]
    public virtual ICollection<UserTb> UserTbs { get; set; } = new List<UserTb>();
}
