using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Argus.Data.Migrations
{
    /// <inheritdoc />
    public partial class initialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Path = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastScanDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScanRuns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SecretCount = table.Column<int>(type: "int", nullable: false),
                    ComponentCount = table.Column<int>(type: "int", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Duration = table.Column<TimeSpan>(type: "time", nullable: true),
                    FilesScanned = table.Column<long>(type: "bigint", nullable: true),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScanRuns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScanRuns_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DetectedSecrets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LineNumber = table.Column<int>(type: "int", nullable: false),
                    MaskedValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Hash = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Severity = table.Column<int>(type: "int", nullable: false),
                    RuleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Confidence = table.Column<int>(type: "int", nullable: false),
                    IsFalsePositive = table.Column<bool>(type: "bit", nullable: false),
                    IsReviewed = table.Column<bool>(type: "bit", nullable: false),
                    ScanRunId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetectedSecrets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetectedSecrets_ScanRuns_ScanRunId",
                        column: x => x.ScanRunId,
                        principalTable: "ScanRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SoftwareComponents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Version = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    License = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PackageUrl = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsTransitive = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Homepage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PublisherUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PublishedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    HasKnownVulnerabilities = table.Column<bool>(type: "bit", nullable: false),
                    ScanRunId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParentComponentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoftwareComponents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SoftwareComponents_ScanRuns_ScanRunId",
                        column: x => x.ScanRunId,
                        principalTable: "ScanRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SoftwareComponents_SoftwareComponents_ParentComponentId",
                        column: x => x.ParentComponentId,
                        principalTable: "SoftwareComponents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Vulnerabilities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CveId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Severity = table.Column<int>(type: "int", nullable: false),
                    CvssScore = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CvssVector = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PublishedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReferenceUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SoftwareComponentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vulnerabilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vulnerabilities_SoftwareComponents_SoftwareComponentId",
                        column: x => x.SoftwareComponentId,
                        principalTable: "SoftwareComponents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DetectedSecrets_Confidence",
                table: "DetectedSecrets",
                column: "Confidence");

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

            migrationBuilder.CreateIndex(
                name: "IX_DetectedSecrets_IsReviewed_IsFalsePositive",
                table: "DetectedSecrets",
                columns: new[] { "IsReviewed", "IsFalsePositive" });

            migrationBuilder.CreateIndex(
                name: "IX_DetectedSecrets_RuleId",
                table: "DetectedSecrets",
                column: "RuleId");

            migrationBuilder.CreateIndex(
                name: "IX_DetectedSecrets_ScanRunId",
                table: "DetectedSecrets",
                column: "ScanRunId");

            migrationBuilder.CreateIndex(
                name: "IX_DetectedSecrets_ScanRunId_IsFalsePositive",
                table: "DetectedSecrets",
                columns: new[] { "ScanRunId", "IsFalsePositive" });

            migrationBuilder.CreateIndex(
                name: "IX_DetectedSecrets_ScanRunId_Severity",
                table: "DetectedSecrets",
                columns: new[] { "ScanRunId", "Severity" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_DetectedSecrets_Severity",
                table: "DetectedSecrets",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_DetectedSecrets_Type",
                table: "DetectedSecrets",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_CreatedAt",
                table: "Projects",
                column: "CreatedAt",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_ScanRuns_CreatedAt",
                table: "ScanRuns",
                column: "CreatedAt",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_ScanRuns_ProjectId",
                table: "ScanRuns",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ScanRuns_ProjectId_Status",
                table: "ScanRuns",
                columns: new[] { "ProjectId", "Status" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_ScanRuns_Status",
                table: "ScanRuns",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareComponents_HasKnownVulnerabilities",
                table: "SoftwareComponents",
                column: "HasKnownVulnerabilities");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareComponents_IsTransitive",
                table: "SoftwareComponents",
                column: "IsTransitive");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareComponents_Name_Version",
                table: "SoftwareComponents",
                columns: new[] { "Name", "Version" });

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareComponents_PackageUrl",
                table: "SoftwareComponents",
                column: "PackageUrl",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareComponents_ParentComponentId",
                table: "SoftwareComponents",
                column: "ParentComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareComponents_ScanRunId",
                table: "SoftwareComponents",
                column: "ScanRunId");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareComponents_ScanRunId_HasKnownVulnerabilities",
                table: "SoftwareComponents",
                columns: new[] { "ScanRunId", "HasKnownVulnerabilities" });

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareComponents_ScanRunId_IsTransitive",
                table: "SoftwareComponents",
                columns: new[] { "ScanRunId", "IsTransitive" });

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareComponents_Type",
                table: "SoftwareComponents",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Vulnerabilities_CveId",
                table: "Vulnerabilities",
                column: "CveId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vulnerabilities_Severity",
                table: "Vulnerabilities",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_Vulnerabilities_SoftwareComponentId",
                table: "Vulnerabilities",
                column: "SoftwareComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_Vulnerabilities_SoftwareComponentId_Severity",
                table: "Vulnerabilities",
                columns: new[] { "SoftwareComponentId", "Severity" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DetectedSecrets");

            migrationBuilder.DropTable(
                name: "Vulnerabilities");

            migrationBuilder.DropTable(
                name: "SoftwareComponents");

            migrationBuilder.DropTable(
                name: "ScanRuns");

            migrationBuilder.DropTable(
                name: "Projects");
        }
    }
}
