using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RideMateAPI.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Biography = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    AvatarUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    AverageRating = table.Column<double>(type: "double precision", nullable: false),
                    TrustScore = table.Column<double>(type: "double precision", nullable: false),
                    CompletedRidesCount = table.Column<int>(type: "integer", nullable: false),
                    CancelledRidesCount = table.Column<int>(type: "integer", nullable: false),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    Roles = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DriverVerificationDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentType = table.Column<int>(type: "integer", nullable: false),
                    FileUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    VerificationStatus = table.Column<int>(type: "integer", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriverVerificationDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DriverVerificationDocuments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Rides",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DriverId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartAddress = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    StartLatitude = table.Column<double>(type: "double precision", nullable: false),
                    StartLongitude = table.Column<double>(type: "double precision", nullable: false),
                    DestinationAddress = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DestinationLatitude = table.Column<double>(type: "double precision", nullable: false),
                    DestinationLongitude = table.Column<double>(type: "double precision", nullable: false),
                    DepartureDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AvailableSeats = table.Column<int>(type: "integer", nullable: false),
                    PricePerSeat = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    RideType = table.Column<int>(type: "integer", nullable: false),
                    RideStatus = table.Column<int>(type: "integer", nullable: false),
                    AutoApproveBookings = table.Column<bool>(type: "boolean", nullable: false),
                    SmokingAllowed = table.Column<bool>(type: "boolean", nullable: false),
                    PetsAllowed = table.Column<bool>(type: "boolean", nullable: false),
                    LuggageAllowed = table.Column<bool>(type: "boolean", nullable: false),
                    MusicAllowed = table.Column<bool>(type: "boolean", nullable: false),
                    ConversationAllowed = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rides", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rides_Users_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SavedRoutes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartAddress = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DestinationAddress = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavedRoutes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SavedRoutes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Vehicles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Model = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    SeatsCount = table.Column<int>(type: "integer", nullable: false),
                    VehicleImageUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    LicenseNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RegistrationCertificateUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vehicles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Bookings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RideId = table.Column<Guid>(type: "uuid", nullable: false),
                    PassengerId = table.Column<Guid>(type: "uuid", nullable: false),
                    SeatsReserved = table.Column<int>(type: "integer", nullable: false),
                    PickupPoint = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DropoffPoint = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    BookingStatus = table.Column<int>(type: "integer", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bookings_Rides_RideId",
                        column: x => x.RideId,
                        principalTable: "Rides",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Bookings_Users_PassengerId",
                        column: x => x.PassengerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RideStops",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RideId = table.Column<Guid>(type: "uuid", nullable: false),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RideStops", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RideStops_Rides_RideId",
                        column: x => x.RideId,
                        principalTable: "Rides",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Disputes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Resolution = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Disputes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Disputes_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Disputes_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    PaymentStatus = table.Column<int>(type: "integer", nullable: false),
                    PaymentMethod = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReviewerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReviewedUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: false),
                    Comment = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reviews_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reviews_Users_ReviewedUserId",
                        column: x => x.ReviewedUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reviews_Users_ReviewerId",
                        column: x => x.ReviewerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_BookingStatus",
                table: "Bookings",
                column: "BookingStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_PassengerId",
                table: "Bookings",
                column: "PassengerId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_RideId",
                table: "Bookings",
                column: "RideId");

            migrationBuilder.CreateIndex(
                name: "IX_Disputes_BookingId",
                table: "Disputes",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_Disputes_CreatedByUserId",
                table: "Disputes",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Disputes_Status",
                table: "Disputes",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_DriverVerificationDocuments_UserId",
                table: "DriverVerificationDocuments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DriverVerificationDocuments_VerificationStatus",
                table: "DriverVerificationDocuments",
                column: "VerificationStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_IsRead",
                table: "Notifications",
                column: "IsRead");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_BookingId",
                table: "Payments",
                column: "BookingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_BookingId",
                table: "Reviews",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_ReviewedUserId",
                table: "Reviews",
                column: "ReviewedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_ReviewerId",
                table: "Reviews",
                column: "ReviewerId");

            migrationBuilder.CreateIndex(
                name: "IX_Rides_DepartureDateTime",
                table: "Rides",
                column: "DepartureDateTime");

            migrationBuilder.CreateIndex(
                name: "IX_Rides_DestinationLatitude_DestinationLongitude",
                table: "Rides",
                columns: new[] { "DestinationLatitude", "DestinationLongitude" });

            migrationBuilder.CreateIndex(
                name: "IX_Rides_DriverId",
                table: "Rides",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_Rides_StartLatitude_StartLongitude",
                table: "Rides",
                columns: new[] { "StartLatitude", "StartLongitude" });

            migrationBuilder.CreateIndex(
                name: "IX_RideStops_Order",
                table: "RideStops",
                column: "Order");

            migrationBuilder.CreateIndex(
                name: "IX_RideStops_RideId",
                table: "RideStops",
                column: "RideId");

            migrationBuilder.CreateIndex(
                name: "IX_SavedRoutes_UserId",
                table: "SavedRoutes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_CreatedAt",
                table: "Users",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_TrustScore",
                table: "Users",
                column: "TrustScore");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_UserId",
                table: "Vehicles",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Disputes");

            migrationBuilder.DropTable(
                name: "DriverVerificationDocuments");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "Reviews");

            migrationBuilder.DropTable(
                name: "RideStops");

            migrationBuilder.DropTable(
                name: "SavedRoutes");

            migrationBuilder.DropTable(
                name: "Vehicles");

            migrationBuilder.DropTable(
                name: "Bookings");

            migrationBuilder.DropTable(
                name: "Rides");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
