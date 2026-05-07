using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace montaherul.Migrations
{
    /// <inheritdoc />
    public partial class AddTeacherProfileImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProfileImage",
                table: "TeacherModel",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfileImage",
                table: "TeacherModel");
        }
    }
}
