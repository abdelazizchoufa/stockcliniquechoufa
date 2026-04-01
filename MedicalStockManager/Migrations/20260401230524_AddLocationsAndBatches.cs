using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicalStockManager.Migrations
{
    /// <inheritdoc />
    public partial class AddLocationsAndBatches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DestinationLocationId",
                table: "StockMovements",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SourceLocationId",
                table: "StockMovements",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StockBatchId",
                table: "StockMovements",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    IsCentral = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StockBatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StockItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    LocationId = table.Column<int>(type: "INTEGER", nullable: false),
                    BatchNumber = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockBatches_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockBatches_StockItems_StockItemId",
                        column: x => x.StockItemId,
                        principalTable: "StockItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_DestinationLocationId",
                table: "StockMovements",
                column: "DestinationLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_SourceLocationId",
                table: "StockMovements",
                column: "SourceLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_StockBatchId",
                table: "StockMovements",
                column: "StockBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_Locations_Name",
                table: "Locations",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockBatches_LocationId",
                table: "StockBatches",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_StockBatches_StockItemId",
                table: "StockBatches",
                column: "StockItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_StockMovements_Locations_DestinationLocationId",
                table: "StockMovements",
                column: "DestinationLocationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StockMovements_Locations_SourceLocationId",
                table: "StockMovements",
                column: "SourceLocationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StockMovements_StockBatches_StockBatchId",
                table: "StockMovements",
                column: "StockBatchId",
                principalTable: "StockBatches",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StockMovements_Locations_DestinationLocationId",
                table: "StockMovements");

            migrationBuilder.DropForeignKey(
                name: "FK_StockMovements_Locations_SourceLocationId",
                table: "StockMovements");

            migrationBuilder.DropForeignKey(
                name: "FK_StockMovements_StockBatches_StockBatchId",
                table: "StockMovements");

            migrationBuilder.DropTable(
                name: "StockBatches");

            migrationBuilder.DropTable(
                name: "Locations");

            migrationBuilder.DropIndex(
                name: "IX_StockMovements_DestinationLocationId",
                table: "StockMovements");

            migrationBuilder.DropIndex(
                name: "IX_StockMovements_SourceLocationId",
                table: "StockMovements");

            migrationBuilder.DropIndex(
                name: "IX_StockMovements_StockBatchId",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "DestinationLocationId",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "SourceLocationId",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "StockBatchId",
                table: "StockMovements");
        }
    }
}
