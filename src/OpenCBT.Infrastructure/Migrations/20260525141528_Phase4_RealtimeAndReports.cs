using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenCBT.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase4_RealtimeAndReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CurrentToken",
                table: "Exams",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TokenRequired",
                table: "Exams",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentToken",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "TokenRequired",
                table: "Exams");
        }
    }
}
