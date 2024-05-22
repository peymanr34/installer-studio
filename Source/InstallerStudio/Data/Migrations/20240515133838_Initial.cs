using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstallerStudio.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Publisher = table.Column<string>(type: "TEXT", nullable: true),
                    Website = table.Column<string>(type: "TEXT", nullable: true),
                    Version = table.Column<string>(type: "TEXT", nullable: true),
                    UniqueId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SetupType = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedDateUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedDateUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Setups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Version = table.Column<string>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    FilePath = table.Column<string>(type: "TEXT", nullable: true),
                    Arguments = table.Column<string>(type: "TEXT", nullable: true),
                    IsX86 = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsX64 = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsArm64 = table.Column<bool>(type: "INTEGER", nullable: false),
                    ProjectId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedDateUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedDateUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Setups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Setups_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Additionals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Path = table.Column<string>(type: "TEXT", nullable: true),
                    IsDirectory = table.Column<bool>(type: "INTEGER", nullable: false),
                    SetupId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedDateUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedDateUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Additionals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Additionals_Setups_SetupId",
                        column: x => x.SetupId,
                        principalTable: "Setups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Additionals_SetupId",
                table: "Additionals",
                column: "SetupId");

            migrationBuilder.CreateIndex(
                name: "IX_Setups_ProjectId",
                table: "Setups",
                column: "ProjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Additionals");

            migrationBuilder.DropTable(
                name: "Setups");

            migrationBuilder.DropTable(
                name: "Projects");
        }
    }
}
