using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Argus.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLocationUniqueConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DetectedSecrets_ScanRunId_FilePath_LineNumber",
                table: "DetectedSecrets");

            migrationBuilder.CreateIndex(
                name: "IX_DetectedSecrets_ScanRunId_FilePath_LineNumber",
                table: "DetectedSecrets",
                columns: new[] { "ScanRunId", "FilePath", "LineNumber" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DetectedSecrets_ScanRunId_FilePath_LineNumber",
                table: "DetectedSecrets");

            migrationBuilder.CreateIndex(
                name: "IX_DetectedSecrets_ScanRunId_FilePath_LineNumber",
                table: "DetectedSecrets",
                columns: new[] { "ScanRunId", "FilePath", "LineNumber" },
                unique: true);
        }
    }
}
