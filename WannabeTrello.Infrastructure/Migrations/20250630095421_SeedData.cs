using Microsoft.EntityFrameworkCore.Migrations;
using WannabeTrello.Domain.Enums;

#nullable disable

namespace WannabeTrello.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            long adminRoleId = 1L;
            long userRoleId = 2L;
            long editorRoleId = 3L;
            long viewerRoleId = 4L;

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "Name", "NormalizedName", "ConcurrencyStamp" },
                values: new object[,]
                {
                    { adminRoleId, "Admin", "ADMIN", Guid.NewGuid().ToString() },
                    { userRoleId, "User", "USER", Guid.NewGuid().ToString() },
                    { editorRoleId, "Editor", "EDITOR", Guid.NewGuid().ToString() },
                    { viewerRoleId, "Viewer", "VIEWER", Guid.NewGuid().ToString() }
                });

            // Umetanje podrazumevanog korisnika (testuser)
            var defaultUserId = 1L; // Fiksni ID za našeg test korisnika
            var testUserPasswordHash = "AQAAAAEAACcQAAAAEFC71e8Qx8bF8z3k9R8v7f8e8t8u8v8w8x8y8z8A8B8C8D8E8F8G8H8I8J8K8L8M8N8O8P8Q8R8S8T8U8V8W8X8Y8Z"; // Pre-hešovana lozinka za "TestPass123!"
            var securityStamp = Guid.NewGuid().ToString();
            var concurrencyStamp = Guid.NewGuid().ToString();

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd", "LockoutEnabled", "AccessFailedCount" },
                values: new object[] { defaultUserId, "testuser", "TESTUSER", "test@example.com", "TEST@EXAMPLE.COM", true, testUserPasswordHash, securityStamp, concurrencyStamp, null, false, false, null, true, 0 }
            );

            // Dodela "Admin" uloge podrazumevanom korisniku
            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "UserId", "RoleId" },
                values: new object[] { defaultUserId, adminRoleId }
            );

            // Umetanje projekta
            var defaultProjectId = 101L;
            migrationBuilder.InsertData(
                table: "Projects",
                columns: new[] { "Id", "Name", "Description", "OwnerId", "CreatedAt", "CreatedBy", "LastModifiedAt", "LastModifiedBy" },
                values: new object[] { defaultProjectId, "Moj Prvi Kanban Projekat - PG", "Ovo je moj prvi Kanban projekat za testiranje na PostgreSQL-u.", defaultUserId, DateTime.UtcNow, defaultUserId, null, null }
            );

            // Umetanje člana projekta (ProjectMember)
            migrationBuilder.InsertData(
                table: "ProjectMembers",
                columns: new[] { "ProjectId", "UserId", "Role" },
                values: new object[] { defaultProjectId, defaultUserId, ProjectRole.Owner.ToString() }
            );

            // Umetanje boarda
            var defaultBoardId = 201L;
            migrationBuilder.InsertData(
                table: "Boards",
                columns: new[] { "Id", "Name", "Description", "ProjectId", "CreatedAt", "CreatedBy", "LastModifiedAt", "LastModifiedBy" },
                values: new object[] { defaultBoardId, "Glavni Board - PG", "Glavni board za upravljanje zadacima u projektu na PostgreSQL-u.", defaultProjectId, DateTime.UtcNow, defaultUserId, null, null }
            );

            // Umetanje člana boarda (BoardMember)
            migrationBuilder.InsertData(
                table: "BoardMembers",
                columns: new[] { "BoardId", "UserId", "Role" },
                values: new object[] { defaultBoardId, defaultUserId, BoardRole.Editor.ToString() }
            );

            // Umetanje kolona (eksplicitno dodeljivanje ID-jeva)
            var toDoColumnId = 301L;
            var inProgressColumnId = 302L;
            var doneColumnId = 303L;

            migrationBuilder.InsertData(
                table: "Columns",
                columns: new[] { "Id", "Name", "Order", "BoardId", "CreatedAt", "CreatedBy", "LastModifiedAt", "LastModifiedBy" },
                values: new object[] { toDoColumnId, "To Do", 1, defaultBoardId, DateTime.UtcNow, defaultUserId, null, null }
            );
            migrationBuilder.InsertData(
                table: "Columns",
                columns: new[] { "Id", "Name", "Order", "BoardId", "CreatedAt", "CreatedBy", "LastModifiedAt", "LastModifiedBy" },
                values: new object[] { inProgressColumnId, "In Progress", 2, defaultBoardId, DateTime.UtcNow, defaultUserId, null, null }
            );
            migrationBuilder.InsertData(
                table: "Columns",
                columns: new[] { "Id", "Name", "Order", "BoardId", "CreatedAt", "CreatedBy", "LastModifiedAt", "LastModifiedBy" },
                values: new object[] { doneColumnId, "Done", 3, defaultBoardId, DateTime.UtcNow, defaultUserId, null, null }
            );

            // Umetanje zadataka
            // AŽURIRANO: Dodata "Position" kolona sa vrednošću
            var task1Id = 401L;
            migrationBuilder.InsertData(
                table: "Tasks",
                columns: new[] { "Id", "Title", "Description", "Priority", "DueDate", "ColumnId", "AssigneeId", "Position", "CreatedAt", "CreatedBy", "LastModifiedAt", "LastModifiedBy" }, // Dodato "Position"
                values: new object[] { task1Id, "Implementiraj autentifikaciju PG", "Dodaj podršku za registraciju i prijavljivanje korisnika sa JWT tokenima za PG.", TaskPriority.High.ToString(), DateTime.UtcNow.AddDays(7), toDoColumnId, defaultUserId, 0, DateTime.UtcNow, defaultUserId, null, null } // Dodato 0 za Position
            );

            var task2Id = 402L;
            migrationBuilder.InsertData(
                table: "Tasks",
                columns: new[] { "Id", "Title", "Description", "Priority", "DueDate", "ColumnId", "AssigneeId", "Position", "CreatedAt", "CreatedBy", "LastModifiedAt", "LastModifiedBy" }, // Dodato "Position"
                values: new object[] { task2Id, "Dizajniraj PG bazu podataka", "Finaliziraj ER dijagram i šemu PostgreSQL baze podataka.", TaskPriority.Medium.ToString(), DateTime.UtcNow.AddDays(3), inProgressColumnId, null, 0, DateTime.UtcNow, defaultUserId, null, null } // Dodato 0 za Position
            );

            var task3Id = 403L;
            migrationBuilder.InsertData(
                table: "Tasks",
                columns: new[] { "Id", "Title", "Description", "Priority", "DueDate", "ColumnId", "AssigneeId", "Position", "CreatedAt", "CreatedBy", "LastModifiedAt", "LastModifiedBy" }, // Dodato "Position"
                values: new object[] { task3Id, "Postavi osnovni API", "Kreiraj osnovne CRUD API endpointe za boardove i zadatke.", TaskPriority.Urgent.ToString(), DateTime.UtcNow.AddDays(1), toDoColumnId, defaultUserId, 1, DateTime.UtcNow, defaultUserId, null, null } // Dodato 1 za Position
            );

            var task4Id = 404L;
            migrationBuilder.InsertData(
                table: "Tasks",
                columns: new[] { "Id", "Title", "Description", "Priority", "DueDate", "ColumnId", "AssigneeId", "Position", "CreatedAt", "CreatedBy", "LastModifiedAt", "LastModifiedBy" }, // Dodato "Position"
                values: new object[] { task4Id, "Završi migracije", "Osiguraj da su sve baze podataka migrirane i funkcionalne.", TaskPriority.Low.ToString(), DateTime.UtcNow.AddDays(-2), doneColumnId, defaultUserId, 0, DateTime.UtcNow, defaultUserId, null, null } // Dodato 0 za Position
            );

            // Umetanje komentara za task4
            var comment1Id = 501L;
            migrationBuilder.InsertData(
                table: "Comments",
                columns: new[] { "Id", "Content", "CreatedAt", "TaskId", "UserId", "CreatedBy", "LastModifiedAt", "LastModifiedBy" },
                values: new object[] { comment1Id, "Ovaj zadatak je završen ranije.", DateTime.UtcNow, task4Id, defaultUserId, defaultUserId, null, null }
            );

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(table: "Comments", keyColumn: "Id", keyValues: new object[] { 501L });
            migrationBuilder.DeleteData(table: "Tasks", keyColumn: "Id", keyValues: new object[] { 401L, 402L, 403L, 404L });
            migrationBuilder.DeleteData(table: "Columns", keyColumn: "Id", keyValues: new object[] { 301L, 302L, 303L });
            migrationBuilder.DeleteData(table: "BoardMembers", keyColumns: new[] { "BoardId", "UserId" }, keyValues: new object[,] { { 201L, 1L } }); // Kompozitni ključevi
            migrationBuilder.DeleteData(table: "Boards", keyColumn: "Id", keyValues: new object[] { 201L });
            migrationBuilder.DeleteData(table: "ProjectMembers", keyColumns: new[] { "ProjectId", "UserId" }, keyValues: new object[,] { { 101L, 1L } }); // Kompozitni ključevi
            migrationBuilder.DeleteData(table: "Projects", keyColumn: "Id", keyValues: new object[] { 101L });
            migrationBuilder.DeleteData(table: "AspNetUserRoles", keyColumns: new[] { "UserId", "RoleId" }, keyValues: new object[,] { { 1L, 1L } });
            migrationBuilder.DeleteData(table: "AspNetUsers", keyColumn: "Id", keyValues: new object[] { 1L });
            migrationBuilder.DeleteData(table: "AspNetRoles", keyColumn: "Id", keyValues: new object[] { 1L, 2L, 3L, 4L });
        }
    }
}
