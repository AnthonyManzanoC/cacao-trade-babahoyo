using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrigenCacao.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BusinessCmsAndOperations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PaymentMethod",
                schema: "cacao",
                table: "Sales",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "CashRegisters",
                schema: "cacao",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessDate = table.Column<DateOnly>(type: "date", nullable: false),
                    OpeningBalance = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    CountedClosingBalance = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: true),
                    ExpectedClosingBalance = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: true),
                    ClosingDifference = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    OpenedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ClosedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashRegisters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProcessingBatches",
                schema: "cacao",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Variety = table.Column<int>(type: "integer", nullable: false),
                    InputWetQuintals = table.Column<decimal>(type: "numeric(14,4)", precision: 14, scale: 4, nullable: false),
                    ExpectedDryYieldPercent = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    OutputDryQuintals = table.Column<decimal>(type: "numeric(14,4)", precision: 14, scale: 4, nullable: true),
                    ActualDryYieldPercent = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: true),
                    LossPercent = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: true),
                    InputUnitCost = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    OutputUnitCost = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessingBatches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PublicContents",
                schema: "cacao",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContentKey = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Section = table.Column<int>(type: "integer", nullable: false),
                    Eyebrow = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Title = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: false),
                    Subtitle = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Body = table.Column<string>(type: "text", nullable: false),
                    PrimaryCtaLabel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PrimaryCtaUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SecondaryCtaLabel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SecondaryCtaUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Icon = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicContents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CashMovements",
                schema: "cacao",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CashRegisterId = table.Column<Guid>(type: "uuid", nullable: false),
                    Direction = table.Column<int>(type: "integer", nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReferenceCode = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    PaymentMethod = table.Column<int>(type: "integer", nullable: false),
                    OccurredAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashMovements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CashMovements_CashRegisters_CashRegisterId",
                        column: x => x.CashRegisterId,
                        principalSchema: "cacao",
                        principalTable: "CashRegisters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InventoryLots",
                schema: "cacao",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    PurchaseId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProcessingBatchId = table.Column<Guid>(type: "uuid", nullable: true),
                    Variety = table.Column<int>(type: "integer", nullable: false),
                    State = table.Column<int>(type: "integer", nullable: false),
                    InitialQuantityQuintals = table.Column<decimal>(type: "numeric(14,4)", precision: 14, scale: 4, nullable: false),
                    AvailableQuantityQuintals = table.Column<decimal>(type: "numeric(14,4)", precision: 14, scale: 4, nullable: false),
                    UnitCost = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    HumidityPercent = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ReceivedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryLots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryLots_ProcessingBatches_ProcessingBatchId",
                        column: x => x.ProcessingBatchId,
                        principalSchema: "cacao",
                        principalTable: "ProcessingBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryLots_Purchases_PurchaseId",
                        column: x => x.PurchaseId,
                        principalSchema: "cacao",
                        principalTable: "Purchases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProcessingLotAllocations",
                schema: "cacao",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcessingBatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    InventoryLotId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuantityQuintals = table.Column<decimal>(type: "numeric(14,4)", precision: 14, scale: 4, nullable: false),
                    UnitCost = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessingLotAllocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcessingLotAllocations_InventoryLots_InventoryLotId",
                        column: x => x.InventoryLotId,
                        principalSchema: "cacao",
                        principalTable: "InventoryLots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProcessingLotAllocations_ProcessingBatches_ProcessingBatchId",
                        column: x => x.ProcessingBatchId,
                        principalSchema: "cacao",
                        principalTable: "ProcessingBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SaleLotAllocations",
                schema: "cacao",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SaleId = table.Column<Guid>(type: "uuid", nullable: false),
                    InventoryLotId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuantityQuintals = table.Column<decimal>(type: "numeric(14,4)", precision: 14, scale: 4, nullable: false),
                    UnitCost = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaleLotAllocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SaleLotAllocations_InventoryLots_InventoryLotId",
                        column: x => x.InventoryLotId,
                        principalSchema: "cacao",
                        principalTable: "InventoryLots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SaleLotAllocations_Sales_SaleId",
                        column: x => x.SaleId,
                        principalSchema: "cacao",
                        principalTable: "Sales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CashMovements_CashRegisterId",
                schema: "cacao",
                table: "CashMovements",
                column: "CashRegisterId");

            migrationBuilder.CreateIndex(
                name: "IX_CashMovements_OccurredAtUtc",
                schema: "cacao",
                table: "CashMovements",
                column: "OccurredAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_CashRegisters_BusinessDate",
                schema: "cacao",
                table: "CashRegisters",
                column: "BusinessDate",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryLots_Code",
                schema: "cacao",
                table: "InventoryLots",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryLots_ProcessingBatchId",
                schema: "cacao",
                table: "InventoryLots",
                column: "ProcessingBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryLots_PurchaseId",
                schema: "cacao",
                table: "InventoryLots",
                column: "PurchaseId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryLots_Variety_State_Status_ReceivedAtUtc",
                schema: "cacao",
                table: "InventoryLots",
                columns: new[] { "Variety", "State", "Status", "ReceivedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ProcessingBatches_Code",
                schema: "cacao",
                table: "ProcessingBatches",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProcessingBatches_Status_StartedAtUtc",
                schema: "cacao",
                table: "ProcessingBatches",
                columns: new[] { "Status", "StartedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ProcessingLotAllocations_InventoryLotId",
                schema: "cacao",
                table: "ProcessingLotAllocations",
                column: "InventoryLotId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessingLotAllocations_ProcessingBatchId_InventoryLotId",
                schema: "cacao",
                table: "ProcessingLotAllocations",
                columns: new[] { "ProcessingBatchId", "InventoryLotId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PublicContents_ContentKey",
                schema: "cacao",
                table: "PublicContents",
                column: "ContentKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PublicContents_Section_DisplayOrder",
                schema: "cacao",
                table: "PublicContents",
                columns: new[] { "Section", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_SaleLotAllocations_InventoryLotId",
                schema: "cacao",
                table: "SaleLotAllocations",
                column: "InventoryLotId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleLotAllocations_SaleId_InventoryLotId",
                schema: "cacao",
                table: "SaleLotAllocations",
                columns: new[] { "SaleId", "InventoryLotId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CashMovements",
                schema: "cacao");

            migrationBuilder.DropTable(
                name: "ProcessingLotAllocations",
                schema: "cacao");

            migrationBuilder.DropTable(
                name: "PublicContents",
                schema: "cacao");

            migrationBuilder.DropTable(
                name: "SaleLotAllocations",
                schema: "cacao");

            migrationBuilder.DropTable(
                name: "CashRegisters",
                schema: "cacao");

            migrationBuilder.DropTable(
                name: "InventoryLots",
                schema: "cacao");

            migrationBuilder.DropTable(
                name: "ProcessingBatches",
                schema: "cacao");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                schema: "cacao",
                table: "Sales");
        }
    }
}
