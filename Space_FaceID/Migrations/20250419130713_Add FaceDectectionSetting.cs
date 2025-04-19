using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Space_FaceID.Migrations
{
    /// <inheritdoc />
    public partial class AddFaceDectectionSetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FaceDetectionSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FaceSize = table.Column<int>(type: "INTEGER", nullable: false),
                    DetectionThreshold = table.Column<float>(type: "REAL", nullable: false),
                    MaxWidth = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxHeight = table.Column<int>(type: "INTEGER", nullable: false),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaceDetectionSettings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FaceDetectionSettings");
        }
    }
}
