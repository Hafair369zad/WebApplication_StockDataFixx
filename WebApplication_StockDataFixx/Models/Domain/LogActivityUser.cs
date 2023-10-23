using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication_StockDataFixx.Models.Domain;

[Table("LOG_ACTIVITY_USER")]
public partial class LogActivityUser
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("USER_ID")]
    [StringLength(30)]
    public string UserId { get; set; } = null!;

    [Column("ACTIVITY")]
    public string Activity { get; set; } = null!;

    [Column("ACTIVITY_TIME", TypeName = "datetime")]
    public DateTime ActivityTime { get; set; }
}
