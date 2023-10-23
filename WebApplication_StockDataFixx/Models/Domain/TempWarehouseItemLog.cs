using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication_StockDataFixx.Models.Domain;

[Table("TEMP_WAREHOUSE_ITEM_LOG")]
public partial class TempWarehouseItemLog
{
    [Key]
    [Column("WAREHOUSE_LOG_ID")]
    [StringLength(225)]
    public string WarehouseLogId { get; set; } = null!;

    [Column("MESSAGE_LOG")]
    public string MessageLog { get; set; } = null!;

    [Column("LOG_TIME", TypeName = "datetime")]
    public DateTime LogTime { get; set; }

    [Column("ISVMI")]
    public string Isvmi { get; set; } = null!;
}
