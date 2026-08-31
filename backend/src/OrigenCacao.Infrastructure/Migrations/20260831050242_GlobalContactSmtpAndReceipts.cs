using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrigenCacao.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class GlobalContactSmtpAndReceipts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomerEmail",
                schema: "cacao",
                table: "Sales",
                type: "character varying(180)",
                maxLength: 180,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                schema: "cacao",
                table: "Producers",
                type: "character varying(180)",
                maxLength: 180,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactAddress",
                schema: "cacao",
                table: "BusinessSettings",
                type: "character varying(400)",
                maxLength: 400,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContactEmail",
                schema: "cacao",
                table: "BusinessSettings",
                type: "character varying(180)",
                maxLength: 180,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContactPhone",
                schema: "cacao",
                table: "BusinessSettings",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "EmailSendingEnabled",
                schema: "cacao",
                table: "BusinessSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "GoogleMapsEmbedUrl",
                schema: "cacao",
                table: "BusinessSettings",
                type: "character varying(1200)",
                maxLength: 1200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SmtpEmail",
                schema: "cacao",
                table: "BusinessSettings",
                type: "character varying(180)",
                maxLength: 180,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SmtpHost",
                schema: "cacao",
                table: "BusinessSettings",
                type: "character varying(240)",
                maxLength: 240,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SmtpPassword",
                schema: "cacao",
                table: "BusinessSettings",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "SmtpPort",
                schema: "cacao",
                table: "BusinessSettings",
                type: "integer",
                nullable: false,
                defaultValue: 587);

            migrationBuilder.AddColumn<bool>(
                name: "SmtpUseSsl",
                schema: "cacao",
                table: "BusinessSettings",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerEmail",
                schema: "cacao",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "Email",
                schema: "cacao",
                table: "Producers");

            migrationBuilder.DropColumn(
                name: "ContactAddress",
                schema: "cacao",
                table: "BusinessSettings");

            migrationBuilder.DropColumn(
                name: "ContactEmail",
                schema: "cacao",
                table: "BusinessSettings");

            migrationBuilder.DropColumn(
                name: "ContactPhone",
                schema: "cacao",
                table: "BusinessSettings");

            migrationBuilder.DropColumn(
                name: "EmailSendingEnabled",
                schema: "cacao",
                table: "BusinessSettings");

            migrationBuilder.DropColumn(
                name: "GoogleMapsEmbedUrl",
                schema: "cacao",
                table: "BusinessSettings");

            migrationBuilder.DropColumn(
                name: "SmtpEmail",
                schema: "cacao",
                table: "BusinessSettings");

            migrationBuilder.DropColumn(
                name: "SmtpHost",
                schema: "cacao",
                table: "BusinessSettings");

            migrationBuilder.DropColumn(
                name: "SmtpPassword",
                schema: "cacao",
                table: "BusinessSettings");

            migrationBuilder.DropColumn(
                name: "SmtpPort",
                schema: "cacao",
                table: "BusinessSettings");

            migrationBuilder.DropColumn(
                name: "SmtpUseSsl",
                schema: "cacao",
                table: "BusinessSettings");
        }
    }
}
