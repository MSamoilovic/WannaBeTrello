using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Feezbow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRecurringTasks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "NextOccurrence",
                table: "Tasks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ParentTaskId",
                table: "Tasks",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Recurrence_DaysOfWeek",
                table: "Tasks",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Recurrence_EndDate",
                table: "Tasks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Recurrence_Frequency",
                table: "Tasks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Recurrence_Interval",
                table: "Tasks",
                type: "integer",
                nullable: true,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "TaskType",
                table: "Tasks",
                type: "text",
                nullable: false,
                defaultValue: "General");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_ParentTaskId",
                table: "Tasks",
                column: "ParentTaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Tasks_ParentTaskId",
                table: "Tasks",
                column: "ParentTaskId",
                principalTable: "Tasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Tasks_ParentTaskId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_ParentTaskId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "NextOccurrence",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "ParentTaskId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "Recurrence_DaysOfWeek",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "Recurrence_EndDate",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "Recurrence_Frequency",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "Recurrence_Interval",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "TaskType",
                table: "Tasks");
        }
    }
}
