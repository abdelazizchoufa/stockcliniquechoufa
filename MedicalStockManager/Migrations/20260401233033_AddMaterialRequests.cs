using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicalStockManager.Migrations
{
    /// <inheritdoc />
    public partial class AddMaterialRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MaterialRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RequestNumber = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    RequestingServiceId = table.Column<int>(type: "INTEGER", nullable: false),
                    RequestedByUsername = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    ProcessedByUsername = table.Column<string>(type: "TEXT", maxLength: 80, nullable: true),
                    RejectionReason = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaterialRequests_Services_RequestingServiceId",
                        column: x => x.RequestingServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MaterialRequestLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MaterialRequestId = table.Column<int>(type: "INTEGER", nullable: false),
                    StockItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    RequestedQuantity = table.Column<int>(type: "INTEGER", nullable: false),
                    ApprovedQuantity = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialRequestLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaterialRequestLines_MaterialRequests_MaterialRequestId",
                        column: x => x.MaterialRequestId,
                        principalTable: "MaterialRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MaterialRequestLines_StockItems_StockItemId",
                        column: x => x.StockItemId,
                        principalTable: "StockItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MaterialRequestLines_MaterialRequestId",
                table: "MaterialRequestLines",
                column: "MaterialRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialRequestLines_StockItemId",
                table: "MaterialRequestLines",
                column: "StockItemId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialRequests_RequestingServiceId",
                table: "MaterialRequests",
                column: "RequestingServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialRequests_RequestNumber",
                table: "MaterialRequests",
                column: "RequestNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MaterialRequestLines");

            migrationBuilder.DropTable(
                name: "MaterialRequests");
        }
    }
}
