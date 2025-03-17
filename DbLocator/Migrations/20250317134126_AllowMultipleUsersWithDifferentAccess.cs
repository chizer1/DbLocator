using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DbLocator.Migrations
{
    /// <inheritdoc />
    public partial class AllowMultipleUsersWithDifferentAccess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DatabaseUser",
                table: "Database");

            migrationBuilder.DropColumn(
                name: "DatabaseUserPassword",
                table: "Database");

            migrationBuilder.CreateTable(
                name: "DatabaseUser",
                columns: table => new
                {
                    DatabaseUserID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DatabaseID = table.Column<int>(type: "int", nullable: false),
                    UserName = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    UserPassword = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Roles = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DatabaseUser", x => x.DatabaseUserID);
                    table.ForeignKey(
                        name: "FK_DatabaseUser_DatabaseId",
                        column: x => x.DatabaseID,
                        principalTable: "Database",
                        principalColumn: "DatabaseID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DatabaseUser_DatabaseID_Roles",
                table: "DatabaseUser",
                column: "DatabaseID")
                .Annotation("SqlServer:Include", new[] { "Roles" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DatabaseUser");

            migrationBuilder.AddColumn<string>(
                name: "DatabaseUser",
                table: "Database",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DatabaseUserPassword",
                table: "Database",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: true);
        }
    }
}
