using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dwindlist.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddExpand : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Expanded",
                table: "TodoItem",
                type: "nvarchar(1)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Expanded",
                table: "TodoItem");
        }
    }
}
