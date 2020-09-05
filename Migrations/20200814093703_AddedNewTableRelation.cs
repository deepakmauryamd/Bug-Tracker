using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BugTracker.Migrations
{
    public partial class AddedNewTableRelation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserProjectBugMaps",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ProjectBugId = table.Column<int>(nullable: false),
                    ApplicationUserId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProjectBugMaps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserProjectBugMaps_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserProjectBugMaps_ProjectBugs_ProjectBugId",
                        column: x => x.ProjectBugId,
                        principalTable: "ProjectBugs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserProjectBugMaps_ApplicationUserId",
                table: "UserProjectBugMaps",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserProjectBugMaps_ProjectBugId",
                table: "UserProjectBugMaps",
                column: "ProjectBugId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserProjectBugMaps");
        }
    }
}
