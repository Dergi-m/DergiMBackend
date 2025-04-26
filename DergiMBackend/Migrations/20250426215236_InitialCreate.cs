using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DergiMBackend.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Organisation_OrganisationId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Organisation_OrganisationId",
                table: "Projects");

            migrationBuilder.DropTable(
                name: "ApplicationUserProject");

            migrationBuilder.DropIndex(
                name: "IX_Projects_OrganisationId",
                table: "Projects");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Organisation",
                table: "Organisation");

            migrationBuilder.RenameTable(
                name: "Organisation",
                newName: "Organisations");

            migrationBuilder.RenameColumn(
                name: "OrganisationId",
                table: "AspNetUsers",
                newName: "ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUsers_OrganisationId",
                table: "AspNetUsers",
                newName: "IX_AspNetUsers_ProjectId");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Organisations",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Organisations",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<string>(
                name: "UniqueName",
                table: "Organisations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Organisations",
                table: "Organisations",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "OrganisationRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CanAssignTasks = table.Column<bool>(type: "bit", nullable: false),
                    CanCreateTasks = table.Column<bool>(type: "bit", nullable: false),
                    VisibleTags = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrganisationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganisationRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganisationRoles_Organisations_OrganisationId",
                        column: x => x.OrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrganisationMemberships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    OrganisationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganisationMemberships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganisationMemberships_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrganisationMemberships_OrganisationRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "OrganisationRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrganisationMemberships_Organisations_OrganisationId",
                        column: x => x.OrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationMemberships_OrganisationId",
                table: "OrganisationMemberships",
                column: "OrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationMemberships_RoleId",
                table: "OrganisationMemberships",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationMemberships_UserId",
                table: "OrganisationMemberships",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationRoles_OrganisationId",
                table: "OrganisationRoles",
                column: "OrganisationId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Projects_ProjectId",
                table: "AspNetUsers",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Projects_ProjectId",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "OrganisationMemberships");

            migrationBuilder.DropTable(
                name: "OrganisationRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Organisations",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "UniqueName",
                table: "Organisations");

            migrationBuilder.RenameTable(
                name: "Organisations",
                newName: "Organisation");

            migrationBuilder.RenameColumn(
                name: "ProjectId",
                table: "AspNetUsers",
                newName: "OrganisationId");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUsers_ProjectId",
                table: "AspNetUsers",
                newName: "IX_AspNetUsers_OrganisationId");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Organisation",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Organisation",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Organisation",
                table: "Organisation",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "ApplicationUserProject",
                columns: table => new
                {
                    ProjectsId = table.Column<int>(type: "int", nullable: false),
                    UsersId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationUserProject", x => new { x.ProjectsId, x.UsersId });
                    table.ForeignKey(
                        name: "FK_ApplicationUserProject_AspNetUsers_UsersId",
                        column: x => x.UsersId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplicationUserProject_Projects_ProjectsId",
                        column: x => x.ProjectsId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Projects_OrganisationId",
                table: "Projects",
                column: "OrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUserProject_UsersId",
                table: "ApplicationUserProject",
                column: "UsersId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Organisation_OrganisationId",
                table: "AspNetUsers",
                column: "OrganisationId",
                principalTable: "Organisation",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Organisation_OrganisationId",
                table: "Projects",
                column: "OrganisationId",
                principalTable: "Organisation",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
