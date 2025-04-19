using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Space_FaceID.Migrations
{
    /// <inheritdoc />
    public partial class AddCameraSetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CameraSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CameraIndex = table.Column<int>(type: "INTEGER", nullable: false),
                    FrameRate = table.Column<int>(type: "INTEGER", nullable: false),
                    FrameWidth = table.Column<int>(type: "INTEGER", nullable: false),
                    FrameHeight = table.Column<int>(type: "INTEGER", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CameraSettings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CameraSettings");
        }
    }
}
