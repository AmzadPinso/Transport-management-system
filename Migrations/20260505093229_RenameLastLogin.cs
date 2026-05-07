using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Transport_Management_System.Migrations
{
    /// <inheritdoc />
    public partial class RenameLastLogin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastLogin",
                table: "Users",
                newName: "LastLogIN");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastLogIN",
                table: "Users",
                newName: "LastLogin");
        }
    }
}
