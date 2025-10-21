using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAMService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RelationUserAndUserTokenTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_UserTokens_UserId",
                schema: "public",
                table: "UserTokens",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserTokens_Users_UserId",
                schema: "public",
                table: "UserTokens",
                column: "UserId",
                principalSchema: "public",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserTokens_Users_UserId",
                schema: "public",
                table: "UserTokens");

            migrationBuilder.DropIndex(
                name: "IX_UserTokens_UserId",
                schema: "public",
                table: "UserTokens");
        }
    }
}
