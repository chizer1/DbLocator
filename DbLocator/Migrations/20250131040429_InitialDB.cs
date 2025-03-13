using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DbLocator.Migrations
{
    /// <inheritdoc />
    internal partial class InitialDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                    DatabaseServerFullyQualifiedDomainName = table.Column<string>(
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
                    )
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
                    DatabaseUser = table.Column<string>(
                        type: "varchar(50)",
                        unicode: false,
                        maxLength: 50,
                        nullable: true
                    ),
                    DatabaseUserPassword = table.Column<string>(
                        type: "varchar(50)",
                        unicode: false,
                        maxLength: 50,
                        nullable: true
                    ),
                    DatabaseServerID = table.Column<int>(type: "int", nullable: false),
                    DatabaseTypeID = table.Column<byte>(type: "tinyint", nullable: false),
                    DatabaseStatusID = table.Column<byte>(type: "tinyint", nullable: false),
                    UseTrustedConnection = table.Column<bool>(type: "bit", nullable: true)
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Connection");

            migrationBuilder.DropTable(name: "Database");

            migrationBuilder.DropTable(name: "Tenant");

            migrationBuilder.DropTable(name: "DatabaseServer");

            migrationBuilder.DropTable(name: "DatabaseType");
        }
    }
}
