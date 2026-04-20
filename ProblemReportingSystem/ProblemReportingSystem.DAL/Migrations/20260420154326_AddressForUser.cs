using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProblemReportingSystem.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddressForUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "address_id",
                table: "users",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_address_id",
                table: "users",
                column: "address_id");

            migrationBuilder.AddForeignKey(
                name: "FK_users_addresses_address_id",
                table: "users",
                column: "address_id",
                principalTable: "addresses",
                principalColumn: "address_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_users_addresses_address_id",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_users_address_id",
                table: "users");

            migrationBuilder.DropColumn(
                name: "address_id",
                table: "users");
        }
    }
}
