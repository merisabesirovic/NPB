using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RideMateAPI.Data;

#nullable disable

namespace RideMateAPI.Migrations
{
    [DbContext(typeof(RideMateDbContext))]
    [Migration("20260613103000_NormalizeOneTimeRideSeatCounters")]
    public partial class NormalizeOneTimeRideSeatCounters : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE "Rides" AS r
                SET "AvailableSeats" = GREATEST(0, r."AvailableSeats" - approved."ReservedSeats")
                FROM (
                    SELECT b."RideId", SUM(b."SeatsReserved")::integer AS "ReservedSeats"
                    FROM "Bookings" AS b
                    INNER JOIN "Rides" AS ride ON ride."Id" = b."RideId"
                    WHERE b."BookingStatus" = 1
                      AND ride."IsRecurring" = FALSE
                      AND ride."AutoApproveBookings" = FALSE
                    GROUP BY b."RideId"
                ) AS approved
                WHERE r."Id" = approved."RideId";
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
