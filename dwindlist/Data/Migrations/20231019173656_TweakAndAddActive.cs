using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dwindlist.Data.Migrations
{
    /// <inheritdoc />
    public partial class TweakAndAddActive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TodoItem_UserId",
                table: "TodoItem");

            migrationBuilder.AddColumn<string>(
                name: "Active",
                table: "TodoItem",
                type: "nvarchar(1)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_TodoItem_UserId_Active",
                table: "TodoItem",
                columns: new[] { "UserId", "Active" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TodoItem_UserId_Active",
                table: "TodoItem");

            migrationBuilder.DropColumn(
                name: "Active",
                table: "TodoItem");

            migrationBuilder.CreateIndex(
                name: "IX_TodoItem_UserId",
                table: "TodoItem",
                column: "UserId");
        }
    }
}
