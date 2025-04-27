using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DergiMBackend.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOrganisationAndMembershipFixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "Organisations",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Organisations_OwnerId",
                table: "Organisations",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Organisations_AspNetUsers_OwnerId",
                table: "Organisations",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Organisations_AspNetUsers_OwnerId",
                table: "Organisations");

            migrationBuilder.DropIndex(
                name: "IX_Organisations_OwnerId",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Organisations");
        }
    }
}
