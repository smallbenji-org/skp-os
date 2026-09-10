using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SKP.OS.Base.Migrations
{
    /// <inheritdoc />
    public partial class AddInstructorToFFEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InstructorProfileId",
                table: "FFEntries",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FFEntries_InstructorProfileId",
                table: "FFEntries",
                column: "InstructorProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_FFEntries_InstructorProfiles_InstructorProfileId",
                table: "FFEntries",
                column: "InstructorProfileId",
                principalTable: "InstructorProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FFEntries_InstructorProfiles_InstructorProfileId",
                table: "FFEntries");

            migrationBuilder.DropIndex(
                name: "IX_FFEntries_InstructorProfileId",
                table: "FFEntries");

            migrationBuilder.DropColumn(
                name: "InstructorProfileId",
                table: "FFEntries");
        }
    }
}
