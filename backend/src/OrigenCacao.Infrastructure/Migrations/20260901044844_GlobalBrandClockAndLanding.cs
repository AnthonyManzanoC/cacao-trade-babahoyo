using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrigenCacao.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class GlobalBrandClockAndLanding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                schema: "cacao",
                table: "BusinessSettings",
                type: "character varying(1200)",
                maxLength: 1200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PriceClockLabel",
                schema: "cacao",
                table: "BusinessSettings",
                type: "character varying(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TimeZone",
                schema: "cacao",
                table: "BusinessSettings",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogoUrl",
                schema: "cacao",
                table: "BusinessSettings");

            migrationBuilder.DropColumn(
                name: "PriceClockLabel",
                schema: "cacao",
                table: "BusinessSettings");

            migrationBuilder.DropColumn(
                name: "TimeZone",
                schema: "cacao",
                table: "BusinessSettings");
        }
    }
}
