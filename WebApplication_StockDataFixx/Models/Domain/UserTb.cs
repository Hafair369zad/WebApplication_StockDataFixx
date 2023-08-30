using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication_StockDataFixx.Models.Domain;

[Table("USER_TB")]
public partial class UserTb
{
    [Column("LEVEL_ID")]
    public int LevelId { get; set; }

    [Key]
    [Column("USER_ID")]
    [StringLength(30)]
    public string UserId { get; set; } = null!;

    [Column("USERNAME")]
    [StringLength(30)]
    public string Username { get; set; } = null!;

    [Column("PASSWORD")]
    [StringLength(20)]
    public string Password { get; set; } = null!;

    [Column("JOB_ID")]
    [StringLength(4)]
    public string JobId { get; set; } = null!;

    [Column("PLANT_ID")]
    [StringLength(4)]
    public string PlantId { get; set; } = null!;

    [Column("ACCESS_PLANT")]
    [StringLength(10)]
    public string? AccessPlant { get; set; }

    [Column("STAT")]
    public int Stat { get; set; }

    [Column("CREATED_DATE_TIME", TypeName = "datetime")]
    public DateTime CreatedDateTime { get; set; }

    [Column("DELETED_DATE_TIME", TypeName = "datetime")]
    public DateTime? DeletedDateTime { get; set; }

    [ForeignKey("AccessPlant")]
    [InverseProperty("UserTbs")]
    public virtual AccessPlantTb? AccessPlantNavigation { get; set; }

    [InverseProperty("User")]
    public virtual ICollection<AccessTb> AccessTbs { get; set; } = new List<AccessTb>();

    [ForeignKey("JobId")]
    [InverseProperty("UserTbs")]
    public virtual JobTb Job { get; set; } = null!;

    [ForeignKey("LevelId")]
    [InverseProperty("UserTbs")]
    public virtual SecurityTb Level { get; set; } = null!;

    [ForeignKey("PlantId")]
    [InverseProperty("UserTbs")]
    public virtual PlantTb Plant { get; set; } = null!;
}
