using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RideMateAPI.Migrations
{
    /// <inheritdoc />
    public partial class LOt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("cdde7cd7-9ee3-4b8c-9d2e-fb935fd3187a"));

            migrationBuilder.AddColumn<DateTime>(
                name: "DestinationDateTime",
                table: "Rides",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsRecurring",
                table: "Rides",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "RecurringEndDate",
                table: "Rides",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RecurringType",
                table: "Rides",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "RideDate",
                table: "Bookings",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AvatarUrl", "AverageRating", "Biography", "CancelledRidesCount", "CompletedRidesCount", "CreatedAt", "DateOfBirth", "Email", "FirstName", "IsVerified", "LastName", "PasswordHash", "PhoneNumber", "Roles", "TrustScore" },
                values: new object[] { new Guid("c525a5e1-7f99-496f-a776-500d5fc595f3"), "", 0.0, "", 0, 0, new DateTime(2026, 6, 12, 8, 5, 29, 877, DateTimeKind.Utc).AddTicks(9906), new DateTime(2026, 6, 12, 8, 5, 29, 877, DateTimeKind.Utc).AddTicks(9902), "admin@localhost", "admin", true, "admin", "AQAAAAIAAYagAAAAEMK+GypQqgRPMqOgxxpnpyirP8zli3qGDaxPEh85qxpKxlgFKHFNNTRwKaoQuvkf9Q==", "", 6, 0.0 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("c525a5e1-7f99-496f-a776-500d5fc595f3"));

            migrationBuilder.DropColumn(
                name: "DestinationDateTime",
                table: "Rides");

            migrationBuilder.DropColumn(
                name: "IsRecurring",
                table: "Rides");

            migrationBuilder.DropColumn(
                name: "RecurringEndDate",
                table: "Rides");

            migrationBuilder.DropColumn(
                name: "RecurringType",
                table: "Rides");

            migrationBuilder.DropColumn(
                name: "RideDate",
                table: "Bookings");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AvatarUrl", "AverageRating", "Biography", "CancelledRidesCount", "CompletedRidesCount", "CreatedAt", "DateOfBirth", "Email", "FirstName", "IsVerified", "LastName", "PasswordHash", "PhoneNumber", "Roles", "TrustScore" },
                values: new object[] { new Guid("cdde7cd7-9ee3-4b8c-9d2e-fb935fd3187a"), "", 0.0, "", 0, 0, new DateTime(2026, 6, 11, 12, 42, 50, 768, DateTimeKind.Utc).AddTicks(1488), new DateTime(2026, 6, 11, 12, 42, 50, 768, DateTimeKind.Utc).AddTicks(1480), "admin@localhost", "admin", true, "admin", "AQAAAAIAAYagAAAAEBT2koCL9MJGujbsV8LwhF6ej1Ox9OLrVT/LM0OZgtQWyEz27J0+xvc+9dvW7Oj3CQ==", "", 6, 0.0 });
        }
    }
}
