using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Space_FaceID.Migrations
{
    /// <inheritdoc />
    public partial class AddFaceRecognizeSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FaceRecognitionModels");

            migrationBuilder.CreateTable(
                name: "FaceRecognizeSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    RecognizerType = table.Column<string>(type: "TEXT", nullable: false),
                    LandmarkType = table.Column<string>(type: "TEXT", nullable: false),
                    RecognizeThreshold = table.Column<float>(type: "REAL", precision: 10, scale: 2, nullable: false),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaceRecognizeSettings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FaceRecognizeSettings");

            migrationBuilder.CreateTable(
                name: "FaceRecognitionModels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    ModelData = table.Column<byte[]>(type: "BLOB", nullable: true),
                    ModelName = table.Column<string>(type: "TEXT", nullable: false),
                    ModelPath = table.Column<string>(type: "TEXT", nullable: true),
                    ModelVersion = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaceRecognitionModels", x => x.Id);
                });
        }
    }
}
