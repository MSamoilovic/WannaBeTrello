using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WannabeTrello.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Added_Jsonb_ : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
    ALTER TABLE ""ActivityTrackers""
    ALTER COLUMN ""OldValue"" TYPE jsonb
    USING ""OldValue""::jsonb;

    ALTER TABLE ""ActivityTrackers""
    ALTER COLUMN ""NewValue"" TYPE jsonb
    USING ""NewValue""::jsonb;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
    ALTER TABLE ""ActivityTrackers""
    ALTER COLUMN ""OldValue"" TYPE text;

    ALTER TABLE ""ActivityTrackers""
    ALTER COLUMN ""NewValue"" TYPE text;
");
        }
    }
}
