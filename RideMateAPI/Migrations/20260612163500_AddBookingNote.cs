using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RideMateAPI.Data;

#nullable disable

namespace RideMateAPI.Migrations
{
    [DbContext(typeof(RideMateDbContext))]
    [Migration("20260612163500_AddBookingNote")]
    public partial class AddBookingNote : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "Bookings",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(
                "UPDATE \"Users\" SET \"Roles\" = 4 WHERE \"Id\" = 'c525a5e1-7f99-496f-a776-500d5fc595f3';");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Note",
                table: "Bookings");

            migrationBuilder.Sql(
                "UPDATE \"Users\" SET \"Roles\" = 6 WHERE \"Id\" = 'c525a5e1-7f99-496f-a776-500d5fc595f3';");
        }
    }
}
