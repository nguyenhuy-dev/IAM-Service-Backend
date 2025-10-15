using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IAMService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitalCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "Privileges",
                schema: "public",
                columns: table => new
                {
                    PrivilegeId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PrivilegeName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Privileges", x => x.PrivilegeId);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                schema: "public",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleName = table.Column<string>(type: "text", nullable: false),
                    RoleCode = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.RoleId);
                });

            migrationBuilder.CreateTable(
                name: "PrivilegeRole",
                schema: "public",
                columns: table => new
                {
                    PrivilegesPrivilegeId = table.Column<int>(type: "integer", nullable: false),
                    RolesRoleId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrivilegeRole", x => new { x.PrivilegesPrivilegeId, x.RolesRoleId });
                    table.ForeignKey(
                        name: "FK_PrivilegeRole_Privileges_PrivilegesPrivilegeId",
                        column: x => x.PrivilegesPrivilegeId,
                        principalSchema: "public",
                        principalTable: "Privileges",
                        principalColumn: "PrivilegeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PrivilegeRole_Roles_RolesRoleId",
                        column: x => x.RolesRoleId,
                        principalSchema: "public",
                        principalTable: "Roles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "public",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    HashedPassword = table.Column<string>(type: "text", nullable: false),
                    Gender = table.Column<bool>(type: "boolean", nullable: false),
                    IdentityNumber = table.Column<string>(type: "text", nullable: false),
                    Age = table.Column<int>(type: "integer", nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_Users_Roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "public",
                        principalTable: "Roles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PrivilegeRole_RolesRoleId",
                schema: "public",
                table: "PrivilegeRole",
                column: "RolesRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                schema: "public",
                table: "Users",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PrivilegeRole",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Privileges",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Roles",
                schema: "public");
        }
    }
}
