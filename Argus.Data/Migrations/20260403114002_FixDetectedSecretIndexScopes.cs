using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Argus.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixDetectedSecretIndexScopes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DetectedSecrets_FilePath_LineNumber",
                table: "DetectedSecrets");

            migrationBuilder.DropIndex(
                name: "IX_DetectedSecrets_Hash",
                table: "DetectedSecrets");

            migrationBuilder.CreateIndex(
                name: "IX_DetectedSecrets_ScanRunId_FilePath_LineNumber",
                table: "DetectedSecrets",
                columns: new[] { "ScanRunId", "FilePath", "LineNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DetectedSecrets_ScanRunId_Hash",
                table: "DetectedSecrets",
                columns: new[] { "ScanRunId", "Hash" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DetectedSecrets_ScanRunId_FilePath_LineNumber",
                table: "DetectedSecrets");

            migrationBuilder.DropIndex(
                name: "IX_DetectedSecrets_ScanRunId_Hash",
                table: "DetectedSecrets");

            migrationBuilder.CreateIndex(
                name: "IX_DetectedSecrets_FilePath_LineNumber",
                table: "DetectedSecrets",
                columns: new[] { "FilePath", "LineNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DetectedSecrets_Hash",
                table: "DetectedSecrets",
                column: "Hash",
                unique: true);
        }
    }
}
