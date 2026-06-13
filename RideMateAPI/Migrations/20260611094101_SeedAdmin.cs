using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RideMateAPI.Migrations
{
    /// <inheritdoc />
    public partial class SeedAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "VehicleImageUrl",
                table: "Vehicles",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<string>(
                name: "RegistrationCertificateUrl",
                table: "Vehicles",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<string>(
                name: "LicenseNumber",
                table: "Vehicles",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "Users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Biography",
                table: "Users",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AlterColumn<string>(
                name: "AvatarUrl",
                table: "Users",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000);

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AvatarUrl", "AverageRating", "Biography", "CancelledRidesCount", "CompletedRidesCount", "CreatedAt", "DateOfBirth", "Email", "FirstName", "IsVerified", "LastName", "PasswordHash", "PhoneNumber", "Roles", "TrustScore" },
                values: new object[] { new Guid("9d091b2c-6b1e-43ad-ab81-00c3531ee172"), "", 0.0, "", 0, 0, new DateTime(2026, 6, 11, 9, 41, 0, 803, DateTimeKind.Utc).AddTicks(6872), new DateTime(2026, 6, 11, 9, 41, 0, 803, DateTimeKind.Utc).AddTicks(6867), "admin@localhost", "admin", true, "admin", "AQAAAAIAAYagAAAAENYyQq2zKv9jzEIu1ZEej4bZlp8fOCZRy7oeK/WrQSchgEnRIpCB+r1aMXjLzo5Fow==", "", 6, 0.0 });

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_LicenseNumber",
                table: "Vehicles",
                column: "LicenseNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Vehicles_LicenseNumber",
                table: "Vehicles");

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("9d091b2c-6b1e-43ad-ab81-00c3531ee172"));

            migrationBuilder.AlterColumn<string>(
                name: "VehicleImageUrl",
                table: "Vehicles",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "RegistrationCertificateUrl",
                table: "Vehicles",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "LicenseNumber",
                table: "Vehicles",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "Users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Biography",
                table: "Users",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "AvatarUrl",
                table: "Users",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldDefaultValue: "");
        }
    }
}
