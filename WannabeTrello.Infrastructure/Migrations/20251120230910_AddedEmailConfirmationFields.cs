using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WannabeTrello.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedEmailConfirmationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmailConfirmationRequestIpAddress",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EmailConfirmationRequestedAt",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EmailConfirmedAt",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailConfirmationRequestIpAddress",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "EmailConfirmationRequestedAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "EmailConfirmedAt",
                table: "AspNetUsers");
        }
    }
}
