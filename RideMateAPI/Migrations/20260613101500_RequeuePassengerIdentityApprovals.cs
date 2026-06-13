using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RideMateAPI.Data;

#nullable disable

namespace RideMateAPI.Migrations
{
    [DbContext(typeof(RideMateDbContext))]
    [Migration("20260613101500_RequeuePassengerIdentityApprovals")]
    public partial class RequeuePassengerIdentityApprovals : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE "DriverVerificationDocuments"
                SET "VerificationStatus" = 0
                WHERE "DocumentType" = 3 AND "VerificationStatus" = 1;
                """);

            migrationBuilder.Sql("""
                UPDATE "Users" AS u
                SET "IsVerified" = FALSE
                WHERE u."IsVerified" = TRUE
                  AND EXISTS (
                      SELECT 1
                      FROM "DriverVerificationDocuments" AS d
                      WHERE d."UserId" = u."Id" AND d."DocumentType" = 3
                  )
                  AND NOT EXISTS (
                      SELECT 1
                      FROM "DriverVerificationDocuments" AS d
                      WHERE d."UserId" = u."Id"
                        AND d."DocumentType" <> 3
                        AND d."VerificationStatus" = 1
                  );
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
