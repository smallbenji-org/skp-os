using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SKP.OS.Base.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectFeedback : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Feedback",
                table: "Projects",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Feedback",
                table: "Projects");
        }
    }
}
