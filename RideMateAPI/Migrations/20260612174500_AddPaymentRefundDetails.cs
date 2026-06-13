using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RideMateAPI.Data;

#nullable disable

namespace RideMateAPI.Migrations
{
    [DbContext(typeof(RideMateDbContext))]
    [Migration("20260612174500_AddPaymentRefundDetails")]
    public partial class AddPaymentRefundDetails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "RefundAmount",
                table: "Payments",
                type: "numeric(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RefundedAt",
                table: "Payments",
                type: "timestamp with time zone",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RefundAmount",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "RefundedAt",
                table: "Payments");
        }
    }
}
