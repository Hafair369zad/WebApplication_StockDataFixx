using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication_StockDataFixx.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductionItems",
                columns: table => new
                {
                    ProductionItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Plant = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sloc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Month = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SerialNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TagNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Material = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaterialDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActualQty = table.Column<int>(type: "int", nullable: false),
                    Qual = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Blocked = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IssuePlanner = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionItems", x => x.ProductionItemId);
                });

            migrationBuilder.CreateTable(
                name: "WarehouseItems",
                columns: table => new
                {
                    WarehouseItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Plant = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sloc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Month = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SerialNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TagNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Material = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaterialDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActualQty = table.Column<int>(type: "int", nullable: false),
                    Qual = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Blocked = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StockType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VendorCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VendorName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IssuePlanner = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsVMI = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarehouseItems", x => x.WarehouseItemId);
                });

            migrationBuilder.CreateTable(
                name: "ManagementItems",
                columns: table => new
                {
                    ManagementItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WarehouseItemId = table.Column<int>(type: "int", nullable: false),
                    ProductionItemId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManagementItems", x => x.ManagementItemId);
                    table.ForeignKey(
                        name: "FK_ManagementItems_ProductionItems_ProductionItemId",
                        column: x => x.ProductionItemId,
                        principalTable: "ProductionItems",
                        principalColumn: "ProductionItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ManagementItems_WarehouseItems_WarehouseItemId",
                        column: x => x.WarehouseItemId,
                        principalTable: "WarehouseItems",
                        principalColumn: "WarehouseItemId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ManagementItems_ProductionItemId",
                table: "ManagementItems",
                column: "ProductionItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ManagementItems_WarehouseItemId",
                table: "ManagementItems",
                column: "WarehouseItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ManagementItems");

            migrationBuilder.DropTable(
                name: "ProductionItems");

            migrationBuilder.DropTable(
                name: "WarehouseItems");
        }
    }
}
