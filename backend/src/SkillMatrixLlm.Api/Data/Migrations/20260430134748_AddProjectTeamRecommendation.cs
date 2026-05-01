using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillMatrixLlm.Api.Data.Migrations;

/// <inheritdoc />
public partial class AddProjectTeamRecommendation : Migration
{
  /// <inheritdoc />
  protected override void Up(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.CreateTable(
        name: "Projects",
        columns: table => new
        {
          Id = table.Column<Guid>(type: "uuid", nullable: false),
          Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
          Description = table.Column<string>(type: "text", nullable: false),
          DesiredTeamSize = table.Column<int>(type: "integer", nullable: false),
          Timeline = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
          Status = table.Column<int>(type: "integer", nullable: false),
          CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
          CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_Projects", x => x.Id);
          table.ForeignKey(
                  name: "FK_Projects_Users_CreatedByUserId",
                  column: x => x.CreatedByUserId,
                  principalTable: "Users",
                  principalColumn: "Id",
                  onDelete: ReferentialAction.Cascade);
        });

    migrationBuilder.CreateTable(
        name: "Teams",
        columns: table => new
        {
          Id = table.Column<Guid>(type: "uuid", nullable: false),
          ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
          Source = table.Column<int>(type: "integer", nullable: false),
          Status = table.Column<int>(type: "integer", nullable: false),
          CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_Teams", x => x.Id);
          table.ForeignKey(
                  name: "FK_Teams_Projects_ProjectId",
                  column: x => x.ProjectId,
                  principalTable: "Projects",
                  principalColumn: "Id",
                  onDelete: ReferentialAction.Cascade);
        });

    migrationBuilder.CreateTable(
        name: "Recommendations",
        columns: table => new
        {
          Id = table.Column<Guid>(type: "uuid", nullable: false),
          ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
          TeamId = table.Column<Guid>(type: "uuid", nullable: false),
          RawResponse = table.Column<string>(type: "jsonb", nullable: false),
          CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_Recommendations", x => x.Id);
          table.ForeignKey(
                  name: "FK_Recommendations_Projects_ProjectId",
                  column: x => x.ProjectId,
                  principalTable: "Projects",
                  principalColumn: "Id",
                  onDelete: ReferentialAction.Cascade);
          table.ForeignKey(
                  name: "FK_Recommendations_Teams_TeamId",
                  column: x => x.TeamId,
                  principalTable: "Teams",
                  principalColumn: "Id",
                  onDelete: ReferentialAction.Cascade);
        });

    migrationBuilder.CreateTable(
        name: "TeamMemberships",
        columns: table => new
        {
          Id = table.Column<Guid>(type: "uuid", nullable: false),
          TeamId = table.Column<Guid>(type: "uuid", nullable: false),
          UserId = table.Column<Guid>(type: "uuid", nullable: false),
          ProjectRole = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
          MembershipStatus = table.Column<int>(type: "integer", nullable: false)
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_TeamMemberships", x => x.Id);
          table.ForeignKey(
                  name: "FK_TeamMemberships_Teams_TeamId",
                  column: x => x.TeamId,
                  principalTable: "Teams",
                  principalColumn: "Id",
                  onDelete: ReferentialAction.Cascade);
          table.ForeignKey(
                  name: "FK_TeamMemberships_Users_UserId",
                  column: x => x.UserId,
                  principalTable: "Users",
                  principalColumn: "Id",
                  onDelete: ReferentialAction.Cascade);
        });

    migrationBuilder.CreateIndex(
        name: "IX_Projects_CreatedByUserId",
        table: "Projects",
        column: "CreatedByUserId");

    migrationBuilder.CreateIndex(
        name: "IX_Recommendations_ProjectId",
        table: "Recommendations",
        column: "ProjectId");

    migrationBuilder.CreateIndex(
        name: "IX_Recommendations_TeamId",
        table: "Recommendations",
        column: "TeamId");

    migrationBuilder.CreateIndex(
        name: "IX_TeamMemberships_TeamId",
        table: "TeamMemberships",
        column: "TeamId");

    migrationBuilder.CreateIndex(
        name: "IX_TeamMemberships_UserId",
        table: "TeamMemberships",
        column: "UserId");

    migrationBuilder.CreateIndex(
        name: "IX_Teams_ProjectId",
        table: "Teams",
        column: "ProjectId");
  }

  /// <inheritdoc />
  protected override void Down(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.DropTable(
        name: "Recommendations");

    migrationBuilder.DropTable(
        name: "TeamMemberships");

    migrationBuilder.DropTable(
        name: "Teams");

    migrationBuilder.DropTable(
        name: "Projects");
  }
}
