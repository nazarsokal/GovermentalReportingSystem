using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProblemReportingSystem.DAL.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TimeConvertion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "appeals_problem_id_fkey",
                table: "appeals");

            migrationBuilder.AlterColumn<Guid>(
                name: "problem_id",
                table: "appeals",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "appeals_problem_id_fkey",
                table: "appeals",
                column: "problem_id",
                principalTable: "problems",
                principalColumn: "problem_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "appeals_problem_id_fkey",
                table: "appeals");

            migrationBuilder.AlterColumn<Guid>(
                name: "problem_id",
                table: "appeals",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "appeals_problem_id_fkey",
                table: "appeals",
                column: "problem_id",
                principalTable: "problems",
                principalColumn: "problem_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
