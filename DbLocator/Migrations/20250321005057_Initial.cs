using DbLocator.Domain;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DbLocator.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DatabaseRole",
                columns: table => new
                {
                    DatabaseRoleID = table
                        .Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DatabaseRoleName = table.Column<string>(
                        type: "varchar(50)",
                        unicode: false,
                        maxLength: 50,
                        nullable: true
                    )
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DatabaseRole", x => x.DatabaseRoleID);
                }
            );

            migrationBuilder.CreateTable(
                name: "DatabaseServer",
                columns: table => new
                {
                    DatabaseServerID = table
                        .Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DatabaseServerName = table.Column<string>(
                        type: "varchar(50)",
                        unicode: false,
                        maxLength: 50,
                        nullable: false
                    ),
                    DatabaseServerHostName = table.Column<string>(
                        type: "varchar(50)",
                        unicode: false,
                        maxLength: 50,
                        nullable: true
                    ),
                    DatabaseServerIPAddress = table.Column<string>(
                        type: "varchar(50)",
                        unicode: false,
                        maxLength: 50,
                        nullable: true
                    ),
                    DatabaseServerFullyQualifiedDomainName = table.Column<string>(
                        type: "varchar(50)",
                        unicode: false,
                        maxLength: 50,
                        nullable: true
                    ),
                    IsLinkedServer = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DatabaseServer", x => x.DatabaseServerID);
                }
            );

            migrationBuilder.CreateTable(
                name: "DatabaseType",
                columns: table => new
                {
                    DatabaseTypeID = table
                        .Column<byte>(type: "tinyint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DatabaseTypeName = table.Column<string>(
                        type: "varchar(20)",
                        unicode: false,
                        maxLength: 20,
                        nullable: false
                    )
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DatabaseType", x => x.DatabaseTypeID);
                }
            );

            migrationBuilder.CreateTable(
                name: "Tenant",
                columns: table => new
                {
                    TenantID = table
                        .Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantName = table.Column<string>(
                        type: "varchar(50)",
                        unicode: false,
                        maxLength: 50,
                        nullable: false
                    ),
                    TenantCode = table.Column<string>(
                        type: "varchar(10)",
                        unicode: false,
                        maxLength: 10,
                        nullable: true
                    ),
                    TenantStatusID = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenant", x => x.TenantID);
                }
            );

            migrationBuilder.CreateTable(
                name: "Database",
                columns: table => new
                {
                    DatabaseID = table
                        .Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DatabaseName = table.Column<string>(
                        type: "varchar(50)",
                        unicode: false,
                        maxLength: 50,
                        nullable: false
                    ),
                    DatabaseServerID = table.Column<int>(type: "int", nullable: false),
                    DatabaseTypeID = table.Column<byte>(type: "tinyint", nullable: false),
                    DatabaseStatusID = table.Column<byte>(type: "tinyint", nullable: false),
                    UseTrustedConnection = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Database", x => x.DatabaseID);
                    table.ForeignKey(
                        name: "FK_Database_DatabaseServer",
                        column: x => x.DatabaseServerID,
                        principalTable: "DatabaseServer",
                        principalColumn: "DatabaseServerID"
                    );
                    table.ForeignKey(
                        name: "FK_Database_DatabaseType",
                        column: x => x.DatabaseTypeID,
                        principalTable: "DatabaseType",
                        principalColumn: "DatabaseTypeID"
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "Connection",
                columns: table => new
                {
                    ConnectionID = table
                        .Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantID = table.Column<int>(type: "int", nullable: false),
                    DatabaseID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Connection", x => x.ConnectionID);
                    table.ForeignKey(
                        name: "FK_Connection_Database",
                        column: x => x.DatabaseID,
                        principalTable: "Database",
                        principalColumn: "DatabaseID"
                    );
                    table.ForeignKey(
                        name: "FK_Connection_Tenant",
                        column: x => x.TenantID,
                        principalTable: "Tenant",
                        principalColumn: "TenantID"
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "DatabaseUser",
                columns: table => new
                {
                    DatabaseUserID = table
                        .Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DatabaseID = table.Column<int>(type: "int", nullable: false),
                    UserName = table.Column<string>(
                        type: "varchar(50)",
                        unicode: false,
                        maxLength: 50,
                        nullable: true
                    ),
                    UserPassword = table.Column<string>(
                        type: "varchar(50)",
                        unicode: false,
                        maxLength: 50,
                        nullable: true
                    )
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DatabaseUser", x => x.DatabaseUserID);
                    table.ForeignKey(
                        name: "FK_DatabaseUser_DatabaseId",
                        column: x => x.DatabaseID,
                        principalTable: "Database",
                        principalColumn: "DatabaseID"
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "DatabaseUserRole",
                columns: table => new
                {
                    DatabaseUserRoleID = table
                        .Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DatabaseRoleID = table.Column<int>(type: "int", nullable: false),
                    DatabaseUserID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DatabaseUserRole", x => x.DatabaseUserRoleID);
                    table.ForeignKey(
                        name: "FK_DatabaseUserRole_DatabaseRole",
                        column: x => x.DatabaseRoleID,
                        principalTable: "DatabaseRole",
                        principalColumn: "DatabaseRoleID"
                    );
                    table.ForeignKey(
                        name: "FK_DatabaseUserRole_DatabaseUser",
                        column: x => x.DatabaseUserID,
                        principalTable: "DatabaseUser",
                        principalColumn: "DatabaseUserID"
                    );
                }
            );

            migrationBuilder.InsertData(
                table: "DatabaseRole",
                columns: ["DatabaseRoleID", "DatabaseRoleName"],
                values: new object[,]
                {
                    { 1, "Owner" },
                    { 2, "SecurityAdmin" },
                    { 3, "AccessAdmin" },
                    { 4, "BackupOperator" },
                    { 5, "DdlAdmin" },
                    { 6, "DataWriter" },
                    { 7, "DataReader" },
                    { 8, "DenyDataWriter" },
                    { 9, "DenyDataReader" }
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_Connection_DatabaseID",
                table: "Connection",
                column: "DatabaseID"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Connection_TenantID",
                table: "Connection",
                column: "TenantID"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Database_DatabaseServerID",
                table: "Database",
                column: "DatabaseServerID"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Database_DatabaseTypeID",
                table: "Database",
                column: "DatabaseTypeID"
            );

            migrationBuilder.CreateIndex(
                name: "IX_DatabaseUser_DatabaseID",
                table: "DatabaseUser",
                column: "DatabaseID"
            );

            migrationBuilder.CreateIndex(
                name: "IX_DatabaseUserRole_DatabaseRoleID",
                table: "DatabaseUserRole",
                column: "DatabaseRoleID"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Connection");

            migrationBuilder.DropTable(name: "DatabaseUserRole");

            migrationBuilder.DropTable(name: "Tenant");

            migrationBuilder.DropTable(name: "DatabaseRole");

            migrationBuilder.DropTable(name: "DatabaseUser");

            migrationBuilder.DropTable(name: "Database");

            migrationBuilder.DropTable(name: "DatabaseServer");

            migrationBuilder.DropTable(name: "DatabaseType");
        }
    }
}
