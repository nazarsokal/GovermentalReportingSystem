using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProblemReportingSystem.DAL.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ImageAsByteArray : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "photo_url",
                table: "problem_photos");

            migrationBuilder.AddColumn<string>(
                name: "content_type",
                table: "problem_photos",
                type: "character varying(6)",
                maxLength: 6,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<byte[]>(
                name: "image_data",
                table: "problem_photos",
                type: "bytea",
                maxLength: 255,
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "content_type",
                table: "problem_photos");

            migrationBuilder.DropColumn(
                name: "image_data",
                table: "problem_photos");

            migrationBuilder.RenameColumn(
                name: "password_hash",
                table: "users",
                newName: "PasswordHash");

            migrationBuilder.AddColumn<string>(
                name: "photo_url",
                table: "problem_photos",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");
        }
    }
}
