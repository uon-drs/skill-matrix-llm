using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillMatrixLlm.Api.Data.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
  /// <inheritdoc />
  protected override void Up(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.CreateTable(
        name: "Skills",
        columns: table => new
        {
          Id = table.Column<Guid>(type: "uuid", nullable: false),
          Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_Skills", x => x.Id);
        });

    migrationBuilder.CreateTable(
        name: "Users",
        columns: table => new
        {
          Id = table.Column<Guid>(type: "uuid", nullable: false),
          KeycloakId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
          DisplayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
          Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
          Role = table.Column<int>(type: "integer", nullable: false)
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_Users", x => x.Id);
        });

    migrationBuilder.CreateTable(
        name: "UserSkills",
        columns: table => new
        {
          Id = table.Column<Guid>(type: "uuid", nullable: false),
          UserId = table.Column<Guid>(type: "uuid", nullable: false),
          SkillId = table.Column<Guid>(type: "uuid", nullable: false),
          Level = table.Column<int>(type: "integer", nullable: false)
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_UserSkills", x => x.Id);
          table.ForeignKey(
                    name: "FK_UserSkills_Skills_SkillId",
                    column: x => x.SkillId,
                    principalTable: "Skills",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
          table.ForeignKey(
                    name: "FK_UserSkills_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
        });

    migrationBuilder.CreateIndex(
        name: "IX_Users_KeycloakId",
        table: "Users",
        column: "KeycloakId",
        unique: true);

    migrationBuilder.CreateIndex(
        name: "IX_UserSkills_SkillId",
        table: "UserSkills",
        column: "SkillId");

    migrationBuilder.CreateIndex(
        name: "IX_UserSkills_UserId_SkillId",
        table: "UserSkills",
        columns: new[] { "UserId", "SkillId" },
        unique: true);
  }

  /// <inheritdoc />
  protected override void Down(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.DropTable(
        name: "UserSkills");

    migrationBuilder.DropTable(
        name: "Skills");

    migrationBuilder.DropTable(
        name: "Users");
  }
}
