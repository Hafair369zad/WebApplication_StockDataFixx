using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using WebApplication_StockDataFixx.Models.Domain;

namespace WebApplication_StockDataFixx.Database;

public partial class PMIDbContext : DbContext
{
    public PMIDbContext()
    {
    }

    public PMIDbContext(DbContextOptions<PMIDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AccessPlantTb> AccessPlantTbs { get; set; }

    public virtual DbSet<AccessTb> AccessTbs { get; set; }

    public virtual DbSet<JobTb> JobTbs { get; set; }

    public virtual DbSet<LogActivityUser> LogActivityUsers { get; set; }

    public virtual DbSet<LogActivityUserMobile> LogActivityUserMobiles { get; set; }

    public virtual DbSet<PlantTb> PlantTbs { get; set; }

    public virtual DbSet<ProductionItem> ProductionItems { get; set; }

    public virtual DbSet<ProductionItemLog> ProductionItemLogs { get; set; }

    public virtual DbSet<ProductionTb> ProductionTbs { get; set; }

    public virtual DbSet<SecurityTb> SecurityTbs { get; set; }

    public virtual DbSet<TempProductionItem> TempProductionItems { get; set; }

    public virtual DbSet<TempWarehouseItem> TempWarehouseItems { get; set; }

    public virtual DbSet<TempWarehouseItemLog> TempWarehouseItemLogs { get; set; }

    public virtual DbSet<UserTb> UserTbs { get; set; }

    public virtual DbSet<WarehouseItem> WarehouseItems { get; set; }

    public virtual DbSet<WarehouseItemLog> WarehouseItemLogs { get; set; }

    public virtual DbSet<WarehouseTb> WarehouseTbs { get; set; }
    public void DeleteFromLog()
    {
        Database.ExecuteSqlRaw("EXEC USP_DELETE_TEMP_WAREHOUSE_ITEM_LOG");
    }

    //    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
    //        => optionsBuilder.UseSqlServer("Server=DESKTOP-L2G797G\\MSSQLSERVER2019;Database=STOCKTAKING_DB;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AccessTb>(entity =>
        {
            entity.Property(e => e.PlantId).IsFixedLength();

            entity.HasOne(d => d.Prod).WithMany(p => p.AccessTbs).HasConstraintName("FK_ACCESS_TB_PRODUCTION_TB");

            entity.HasOne(d => d.User).WithMany(p => p.AccessTbs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ACCESS_TB_USER_TB");

            entity.HasOne(d => d.Wrh).WithMany(p => p.AccessTbs).HasConstraintName("FK_ACCESS_TB_WAREHOUSE_TB");
        });

        modelBuilder.Entity<ProductionItem>(entity =>
        {
            entity.ToTable("PRODUCTION_ITEM", tb =>
                {
                    tb.HasTrigger("TRG_LAST_INPUT_DATA_PRODUCTION_ITEM");
                    tb.HasTrigger("TRG_LAST_UPLOAD_PRODUCTION_ITEM");
                    tb.HasTrigger("TRG_PRODUCTION_ITEM_LOG");
                });

            entity.Property(e => e.LastUpload).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.AccessPlantNavigation).WithMany(p => p.ProductionItems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PRODUCTION_ITEM_ACCESS_PLANT_TB");

            entity.HasOne(d => d.Prod).WithMany(p => p.ProductionItems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PRODUCTION_ITEM_PRODUCTION_TB");
        });

        modelBuilder.Entity<ProductionTb>(entity =>
        {
            entity.Property(e => e.PlantId).IsFixedLength();
        });

        modelBuilder.Entity<SecurityTb>(entity =>
        {
            entity.Property(e => e.LevelId).ValueGeneratedNever();
        });

        modelBuilder.Entity<TempProductionItem>(entity =>
        {
            entity.ToTable("TEMP_PRODUCTION_ITEM", tb =>
                {
                    tb.HasTrigger("TRG_COPY_TO_PRODUCTION_ITEM");
                    tb.HasTrigger("TRG_LAST_INPUT_DATA_TEMP_PRODUCTION_ITEM");
                    tb.HasTrigger("TRG_LAST_UPLOAD_TEMP_PRODUCTION_ITEM");
                    tb.HasTrigger("TRG_OLD_DATA_IN_TEMP_PRODUCTION_ITEM");
                    tb.HasTrigger("TRG_UPDATE_PRODUCTION_ACTUAL_QTY");
                });

            entity.Property(e => e.LastUpload).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.AccessPlantNavigation).WithMany(p => p.TempProductionItems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TEMP_PRODUCTION_ITEM_ACCESS_PLANT_TB");

            entity.HasOne(d => d.Prod).WithMany(p => p.TempProductionItems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TEMP_PRODUCTION_ITEM_PRODUCTION_TB");
        });

        modelBuilder.Entity<TempWarehouseItem>(entity =>
        {
            entity.ToTable("TEMP_WAREHOUSE_ITEM", tb =>
                {
                    tb.HasTrigger("TRG_COPY_TO_WAREHOUSE_ITEM");
                    tb.HasTrigger("TRG_LAST_INPUT_DATA_TEMP_WAREHOUSE_ITEM");
                    tb.HasTrigger("TRG_LAST_UPLOAD_TEMP_WAREHOUSE_ITEM");
                    tb.HasTrigger("TRG_OLD_DATA_IN_TEMP_WAREHOUSE_ITEM");
                    tb.HasTrigger("TRG_TEMP_WAREHOUSE_ITEM_LOG");
                    tb.HasTrigger("TRG_UPDATE_WAREHOUSE_ACTUAL_QTY");
                    tb.HasTrigger("TRG_WAREHOUSE_ITEM_LOG");
                });

            entity.Property(e => e.LastUpload).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.AccessPlantNavigation).WithMany(p => p.TempWarehouseItems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TEMP_WAREHOUSE_ITEM_ACCESS_PLANT_TB");

            entity.HasOne(d => d.Wrh).WithMany(p => p.TempWarehouseItems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TEMP_WAREHOUSE_ITEM_WAREHOUSE_TB");
        });

        modelBuilder.Entity<UserTb>(entity =>
        {
            entity.ToTable("USER_TB", tb =>
                {
                    tb.HasTrigger("TGR_CREATE_ACCESS_PLANT_IN_USER_TB");
                    tb.HasTrigger("TRG_DELETE_ACCESS");
                    tb.HasTrigger("TRG_DELETE_USERID_ACCESS");
                    tb.HasTrigger("TRG_SET_CREATED_DATE_TIME");
                    tb.HasTrigger("TRG_SET_STAT_AKTIF");
                    tb.HasTrigger("TRG_SET_STAT_NONAKTIF_DAN_SET_DELETED_DATE_TIME");
                    tb.HasTrigger("TRG_UPDATE_STAT");
                });

            entity.Property(e => e.CreatedDateTime).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.AccessPlantNavigation).WithMany(p => p.UserTbs).HasConstraintName("FK_USER_TB_ACCESS_PLANT_TB");

            entity.HasOne(d => d.Job).WithMany(p => p.UserTbs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_USER_TB_JOB_TB");

            entity.HasOne(d => d.Level).WithMany(p => p.UserTbs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_USER_TB_SECURITY_TB");

            entity.HasOne(d => d.Plant).WithMany(p => p.UserTbs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_USER_TB_PLANT_TB");
        });

        modelBuilder.Entity<WarehouseItem>(entity =>
        {
            entity.ToTable("WAREHOUSE_ITEM", tb =>
                {
                    tb.HasTrigger("TRG_LAST_INPUT_DATA_WAREHOUSE_ITEM");
                    tb.HasTrigger("TRG_LAST_UPLOAD_WAREHOUSE_ITEM");
                });

            entity.Property(e => e.LastUpload).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.AccessPlantNavigation).WithMany(p => p.WarehouseItems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_WAREHOUSE_ITEM_ACCESS_PLANT_TB");

            entity.HasOne(d => d.Wrh).WithMany(p => p.WarehouseItems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_WAREHOUSE_ITEM_WAREHOUSE_TB");
        });

        modelBuilder.Entity<WarehouseTb>(entity =>
        {
            entity.Property(e => e.PlantId).IsFixedLength();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
