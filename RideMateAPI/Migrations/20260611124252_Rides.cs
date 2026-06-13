using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RideMateAPI.Migrations
{
    /// <inheritdoc />
    public partial class Rides : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("9d091b2c-6b1e-43ad-ab81-00c3531ee172"));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AvatarUrl", "AverageRating", "Biography", "CancelledRidesCount", "CompletedRidesCount", "CreatedAt", "DateOfBirth", "Email", "FirstName", "IsVerified", "LastName", "PasswordHash", "PhoneNumber", "Roles", "TrustScore" },
                values: new object[] { new Guid("cdde7cd7-9ee3-4b8c-9d2e-fb935fd3187a"), "", 0.0, "", 0, 0, new DateTime(2026, 6, 11, 12, 42, 50, 768, DateTimeKind.Utc).AddTicks(1488), new DateTime(2026, 6, 11, 12, 42, 50, 768, DateTimeKind.Utc).AddTicks(1480), "admin@localhost", "admin", true, "admin", "AQAAAAIAAYagAAAAEBT2koCL9MJGujbsV8LwhF6ej1Ox9OLrVT/LM0OZgtQWyEz27J0+xvc+9dvW7Oj3CQ==", "", 6, 0.0 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("cdde7cd7-9ee3-4b8c-9d2e-fb935fd3187a"));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AvatarUrl", "AverageRating", "Biography", "CancelledRidesCount", "CompletedRidesCount", "CreatedAt", "DateOfBirth", "Email", "FirstName", "IsVerified", "LastName", "PasswordHash", "PhoneNumber", "Roles", "TrustScore" },
                values: new object[] { new Guid("9d091b2c-6b1e-43ad-ab81-00c3531ee172"), "", 0.0, "", 0, 0, new DateTime(2026, 6, 11, 9, 41, 0, 803, DateTimeKind.Utc).AddTicks(6872), new DateTime(2026, 6, 11, 9, 41, 0, 803, DateTimeKind.Utc).AddTicks(6867), "admin@localhost", "admin", true, "admin", "AQAAAAIAAYagAAAAENYyQq2zKv9jzEIu1ZEej4bZlp8fOCZRy7oeK/WrQSchgEnRIpCB+r1aMXjLzo5Fow==", "", 6, 0.0 });
        }
    }
}
