using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication_StockDataFixx.Models.Domain;

[Table("JOB_TB")]
public partial class JobTb
{
    [Key]
    [Column("JOB_ID")]
    [StringLength(4)]
    public string JobId { get; set; } = null!;

    [Column("JOB_DESC")]
    public string JobDesc { get; set; } = null!;

    [InverseProperty("Job")]
    public virtual ICollection<UserTb> UserTbs { get; set; } = new List<UserTb>();
}
