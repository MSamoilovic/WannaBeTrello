using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Feezbow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBillRecurrence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "NextOccurrence",
                table: "Bill",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ParentBillId",
                table: "Bill",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bill_NextOccurrence",
                table: "Bill",
                column: "NextOccurrence");

            migrationBuilder.CreateIndex(
                name: "IX_Bill_ParentBillId",
                table: "Bill",
                column: "ParentBillId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bill_Bill_ParentBillId",
                table: "Bill",
                column: "ParentBillId",
                principalTable: "Bill",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bill_Bill_ParentBillId",
                table: "Bill");

            migrationBuilder.DropIndex(
                name: "IX_Bill_NextOccurrence",
                table: "Bill");

            migrationBuilder.DropIndex(
                name: "IX_Bill_ParentBillId",
                table: "Bill");

            migrationBuilder.DropColumn(
                name: "NextOccurrence",
                table: "Bill");

            migrationBuilder.DropColumn(
                name: "ParentBillId",
                table: "Bill");
        }
    }
}
