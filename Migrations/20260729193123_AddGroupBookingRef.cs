using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Transport_Management_System.Migrations
{
    /// <inheritdoc />
    public partial class AddGroupBookingRef : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GroupBookingRef",
                table: "Bookings",
                type: "nvarchar(25)",
                maxLength: 25,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GroupBookingRef",
                table: "Bookings");
        }
    }
}
