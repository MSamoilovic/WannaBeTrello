using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WannabeTrello.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedProjectModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActivityTracker_AspNetUsers_UserId",
                table: "ActivityTracker");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ActivityTracker",
                table: "ActivityTracker");

            migrationBuilder.RenameTable(
                name: "ActivityTracker",
                newName: "ActivityTrackers");

            migrationBuilder.RenameIndex(
                name: "IX_ActivityTracker_UserId",
                table: "ActivityTrackers",
                newName: "IX_ActivityTrackers_UserId");

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "Projects",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Projects",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Visibility",
                table: "Projects",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ActivityTrackers",
                table: "ActivityTrackers",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_IsArchived",
                table: "Projects",
                column: "IsArchived");

            migrationBuilder.AddForeignKey(
                name: "FK_ActivityTrackers_AspNetUsers_UserId",
                table: "ActivityTrackers",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActivityTrackers_AspNetUsers_UserId",
                table: "ActivityTrackers");

            migrationBuilder.DropIndex(
                name: "IX_Projects_IsArchived",
                table: "Projects");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ActivityTrackers",
                table: "ActivityTrackers");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Visibility",
                table: "Projects");

            migrationBuilder.RenameTable(
                name: "ActivityTrackers",
                newName: "ActivityTracker");

            migrationBuilder.RenameIndex(
                name: "IX_ActivityTrackers_UserId",
                table: "ActivityTracker",
                newName: "IX_ActivityTracker_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ActivityTracker",
                table: "ActivityTracker",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ActivityTracker_AspNetUsers_UserId",
                table: "ActivityTracker",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
