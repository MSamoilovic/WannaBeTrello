using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WannabeTrello.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Added_WIPLimit_To_Column : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WipLimit",
                table: "Columns",
                type: "integer",
                nullable: true,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WipLimit",
                table: "Columns");
        }
    }
}
