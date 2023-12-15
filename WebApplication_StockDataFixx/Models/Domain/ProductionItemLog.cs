﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication_StockDataFixx.Models.Domain;

[Table("PRODUCTION_ITEM_LOG")]
public partial class ProductionItemLog
{
    [Key]
    [Column("PRODUCTION_LOG_ID")]
    [StringLength(225)]
    public string ProductionLogId { get; set; } = null!;

    [Column("PLANT")]
    public string Plant { get; set; } = null!;

    [Column("MESSAGE_LOG")]
    public string MessageLog { get; set; } = null!;

    [Column("LOG_TIME", TypeName = "datetime")]
    public DateTime LogTime { get; set; }
}
