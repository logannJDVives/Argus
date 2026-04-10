using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Argus.Data.Migrations
{
    /// <inheritdoc />
    public partial class SoftwareComponents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SoftwareComponents_PackageUrl",
                table: "SoftwareComponents");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareComponents_ScanRunId_PackageUrl",
                table: "SoftwareComponents",
                columns: new[] { "ScanRunId", "PackageUrl" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SoftwareComponents_ScanRunId_PackageUrl",
                table: "SoftwareComponents");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareComponents_PackageUrl",
                table: "SoftwareComponents",
                column: "PackageUrl",
                unique: true);
        }
    }
}
