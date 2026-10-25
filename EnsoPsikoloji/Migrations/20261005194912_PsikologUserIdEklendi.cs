using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnsoPsikoloji.Migrations
{
    /// <inheritdoc />
    public partial class PsikologUserIdEklendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Psikologlar",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Psikologlar");
        }
    }
}
