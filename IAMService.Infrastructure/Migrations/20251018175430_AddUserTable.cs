using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAMService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPatient",
                schema: "public",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NeedsVerification",
                schema: "public",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPatient",
                schema: "public",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "NeedsVerification",
                schema: "public",
                table: "Users");
        }
    }
}
