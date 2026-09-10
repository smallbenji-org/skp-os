using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SKP.OS.Base.Migrations
{
    /// <inheritdoc />
    public partial class AddIsCheckInBlocked : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCheckInBlocked",
                table: "StudentProfiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCheckInBlocked",
                table: "StudentProfiles");
        }
    }
}
