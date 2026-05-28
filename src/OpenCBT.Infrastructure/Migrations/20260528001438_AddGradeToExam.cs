using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenCBT.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGradeToExam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "GradeId",
                table: "Exams",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Exams_GradeId",
                table: "Exams",
                column: "GradeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Exams_Grades_GradeId",
                table: "Exams",
                column: "GradeId",
                principalTable: "Grades",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Exams_Grades_GradeId",
                table: "Exams");

            migrationBuilder.DropIndex(
                name: "IX_Exams_GradeId",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "GradeId",
                table: "Exams");
        }
    }
}
