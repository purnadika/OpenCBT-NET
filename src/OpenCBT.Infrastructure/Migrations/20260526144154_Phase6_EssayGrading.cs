using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenCBT.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase6_EssayGrading : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "SelectedAnswerOptionId",
                table: "StudentResponses",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "EssayAnswer",
                table: "StudentResponses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PointsObtained",
                table: "StudentResponses",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TeacherFeedback",
                table: "StudentResponses",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EssayAnswer",
                table: "StudentResponses");

            migrationBuilder.DropColumn(
                name: "PointsObtained",
                table: "StudentResponses");

            migrationBuilder.DropColumn(
                name: "TeacherFeedback",
                table: "StudentResponses");

            migrationBuilder.AlterColumn<Guid>(
                name: "SelectedAnswerOptionId",
                table: "StudentResponses",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
