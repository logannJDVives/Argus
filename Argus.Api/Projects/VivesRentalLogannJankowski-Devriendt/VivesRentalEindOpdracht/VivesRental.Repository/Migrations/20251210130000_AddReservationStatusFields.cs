using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VivesRental.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddReservationStatusFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ArticleReservation",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<DateTime>(
                name: "ProcessedAt",
                table: "ArticleReservation",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "ArticleReservation",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "ArticleReservation",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ArticleReservation");

            migrationBuilder.DropColumn(
                name: "ProcessedAt",
                table: "ArticleReservation");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "ArticleReservation");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "ArticleReservation");
        }
    }
}
