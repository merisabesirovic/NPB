using Microsoft.EntityFrameworkCore;
using RideMateAPI.Models;

namespace RideMateAPI.Data
{
    public class RideMateDbContext : DbContext
    {
        public RideMateDbContext(DbContextOptions<RideMateDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Ride> Rides { get; set; }
        public DbSet<RideStop> RideStops { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<SavedRoute> SavedRoutes { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Dispute> Disputes { get; set; }
        public DbSet<DriverVerificationDocument> DriverVerificationDocuments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Users
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(256);
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.FirstName).HasMaxLength(100);
                entity.Property(u => u.LastName).HasMaxLength(100);
                entity.Property(u => u.PhoneNumber).HasMaxLength(50);
                entity.Property(u => u.Biography).HasMaxLength(2000).HasDefaultValue(string.Empty);
                entity.Property(u => u.AvatarUrl).HasMaxLength(1000).HasDefaultValue(string.Empty);
                entity.Property(u => u.PhoneNumber).HasMaxLength(50).HasDefaultValue(string.Empty);
                entity.Property(u => u.CreatedAt).HasDefaultValueSql("now()");

                entity.HasMany(u => u.Vehicles).WithOne(v => v.User).HasForeignKey(v => v.UserId).OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(u => u.Rides).WithOne(r => r.Driver).HasForeignKey(r => r.DriverId).OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(u => u.Bookings).WithOne(b => b.Passenger).HasForeignKey(b => b.PassengerId).OnDelete(DeleteBehavior.Restrict);
                entity.HasMany(u => u.SavedRoutes).WithOne(sr => sr.User).HasForeignKey(sr => sr.UserId).OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(u => u.Notifications).WithOne(n => n.User).HasForeignKey(n => n.UserId).OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(u => u.RefreshTokens).WithOne(rt => rt.User).HasForeignKey(rt => rt.UserId).OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(u => u.DriverVerificationDocuments).WithOne(d => d.User).HasForeignKey(d => d.UserId).OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(u => u.ReviewsWritten).WithOne(r => r.Reviewer).HasForeignKey(r => r.ReviewerId).OnDelete(DeleteBehavior.Restrict);
                entity.HasMany(u => u.ReviewsReceived).WithOne(r => r.ReviewedUser).HasForeignKey(r => r.ReviewedUserId).OnDelete(DeleteBehavior.Restrict);
                entity.HasMany(u => u.Disputes).WithOne(d => d.CreatedByUser).HasForeignKey(d => d.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(u => u.CreatedAt);
                entity.HasIndex(u => u.TrustScore);
            });

            // Vehicle
            modelBuilder.Entity<Vehicle>(entity =>
            {
                entity.HasKey(v => v.Id);
                entity.Property(v => v.Model).HasMaxLength(200);
                entity.Property(v => v.VehicleImageUrl).HasMaxLength(1000).HasDefaultValue(string.Empty);
                entity.Property(v => v.LicenseNumber).HasMaxLength(100).HasDefaultValue(string.Empty);
                entity.Property(v => v.RegistrationCertificateUrl).HasMaxLength(1000).HasDefaultValue(string.Empty);
                entity.HasIndex(v => v.UserId);
            });

            // Ride
            modelBuilder.Entity<Ride>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.IsRecurring).HasDefaultValue(false);
                entity.Property(r => r.RecurringType).HasDefaultValue(Models.RecurringType.Daily);
                entity.Property(r => r.RecurringEndDate).IsRequired(false);
                entity.Property(r => r.StartAddress).HasMaxLength(500);
                entity.Property(r => r.DestinationAddress).HasMaxLength(500);
                entity.Property(r => r.PricePerSeat).HasColumnType("numeric(10,2)");
                entity.Property(r => r.CreatedAt).HasDefaultValueSql("now()");
                entity.HasIndex(r => r.DriverId);
                entity.HasIndex(r => r.DepartureDateTime);
                entity.HasIndex(r => new { r.StartLatitude, r.StartLongitude });
                entity.HasIndex(r => new { r.DestinationLatitude, r.DestinationLongitude });

                entity.HasMany(r => r.RideStops).WithOne(rs => rs.Ride).HasForeignKey(rs => rs.RideId).OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(r => r.Bookings).WithOne(b => b.Ride).HasForeignKey(b => b.RideId).OnDelete(DeleteBehavior.Cascade);
            });

            // RideStop
            modelBuilder.Entity<RideStop>(entity =>
            {
                entity.HasKey(rs => rs.Id);
                entity.Property(rs => rs.Address).HasMaxLength(500);
                entity.HasIndex(rs => rs.RideId);
                entity.HasIndex(rs => rs.Order);
            });

            // Booking
            modelBuilder.Entity<Booking>(entity =>
            {
                entity.HasKey(b => b.Id);
                entity.Property(b => b.PickupPoint).HasMaxLength(500);
                entity.Property(b => b.DropoffPoint).HasMaxLength(500);
                entity.Property(b => b.Note).HasMaxLength(1000).HasDefaultValue(string.Empty);
                entity.Property(b => b.TotalPrice).HasColumnType("numeric(10,2)");
                entity.Property(b => b.CreatedAt).HasDefaultValueSql("now()");

                entity.HasOne(b => b.Payment).WithOne(p => p.Booking).HasForeignKey<Payment>(p => p.BookingId).OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(b => b.Reviews).WithOne(r => r.Booking).HasForeignKey(r => r.BookingId).OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(b => b.Disputes).WithOne(d => d.Booking).HasForeignKey(d => d.BookingId).OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(b => b.RideId);
                entity.HasIndex(b => b.PassengerId);
                entity.HasIndex(b => b.BookingStatus);
            });

            // Payment
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Amount).HasColumnType("numeric(10,2)");
                entity.Property(p => p.RefundAmount).HasColumnType("numeric(10,2)");
                entity.Property(p => p.CreatedAt).HasDefaultValueSql("now()");
                entity.HasIndex(p => p.BookingId).IsUnique();
            });

            // Review
            modelBuilder.Entity<Review>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Rating).IsRequired();
                entity.Property(r => r.Comment).HasMaxLength(2000);
                entity.Property(r => r.CreatedAt).HasDefaultValueSql("now()");

                entity.HasIndex(r => r.ReviewedUserId);
                entity.HasIndex(r => r.ReviewerId);
                entity.HasIndex(r => r.BookingId);
            });

            // SavedRoute
            modelBuilder.Entity<SavedRoute>(entity =>
            {
                entity.HasKey(sr => sr.Id);
                entity.Property(sr => sr.StartAddress).HasMaxLength(500);
                entity.Property(sr => sr.DestinationAddress).HasMaxLength(500);
                entity.HasIndex(sr => sr.UserId);
            });

            // Notification
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(n => n.Id);
                entity.Property(n => n.Title).HasMaxLength(200);
                entity.Property(n => n.Message).HasMaxLength(2000);
                entity.Property(n => n.CreatedAt).HasDefaultValueSql("now()");
                entity.HasIndex(n => n.UserId);
                entity.HasIndex(n => n.IsRead);
            });

            // Refresh token revoke list
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(rt => rt.Id);
                entity.Property(rt => rt.TokenHash).IsRequired().HasMaxLength(128);
                entity.Property(rt => rt.CreatedByIp).HasMaxLength(100).HasDefaultValue(string.Empty);
                entity.Property(rt => rt.RevokedByIp).HasMaxLength(100).HasDefaultValue(string.Empty);
                entity.Property(rt => rt.ReplacedByTokenHash).HasMaxLength(128).HasDefaultValue(string.Empty);
                entity.Property(rt => rt.CreatedAt).HasDefaultValueSql("now()");
                entity.HasIndex(rt => rt.TokenHash).IsUnique();
                entity.HasIndex(rt => rt.UserId);
                entity.HasIndex(rt => rt.ExpiresAt);
                entity.HasIndex(rt => rt.RevokedAt);
            });

            // Dispute
            modelBuilder.Entity<Dispute>(entity =>
            {
                entity.HasKey(d => d.Id);
                entity.Property(d => d.Description).HasMaxLength(2000);
                entity.Property(d => d.Resolution).HasMaxLength(2000);
                entity.Property(d => d.CreatedAt).HasDefaultValueSql("now()");

                entity.HasIndex(d => d.BookingId);
                entity.HasIndex(d => d.CreatedByUserId);
                entity.HasIndex(d => d.Status);
            });

            // DriverVerificationDocument
            modelBuilder.Entity<DriverVerificationDocument>(entity =>
            {
                entity.HasKey(d => d.Id);
                entity.Property(d => d.FileUrl).HasMaxLength(1000);
                entity.Property(d => d.UploadedAt).HasDefaultValueSql("now()");
                entity.HasIndex(d => d.UserId);
                entity.HasIndex(d => d.VerificationStatus);
            });

            // Vehicle license number index
            modelBuilder.Entity<Vehicle>(entity =>
            {
                entity.HasIndex(v => v.LicenseNumber);
            });

            // PostgreSQL specific suggestions: use GIN indexes for text search or geography if used. Example: index on email or start/destination addresses could be GIN on lower(email) if configured via migrations.

            // Seed admin user
            var admin = new User
            {
                Id = new Guid("c525a5e1-7f99-496f-a776-500d5fc595f3"),
                FirstName = "admin",
                LastName = "admin",
                Email = "admin@localhost",
                PasswordHash = "AQAAAAIAAYagAAAAEMK+GypQqgRPMqOgxxpnpyirP8zli3qGDaxPEh85qxpKxlgFKHFNNTRwKaoQuvkf9Q==",
                PhoneNumber = string.Empty,
                DateOfBirth = new DateTime(2026, 6, 12, 8, 5, 29, 877, DateTimeKind.Utc).AddTicks(9902),
                Biography = string.Empty,
                AvatarUrl = string.Empty,
                CreatedAt = new DateTime(2026, 6, 12, 8, 5, 29, 877, DateTimeKind.Utc).AddTicks(9906),
                Roles = UserRole.Admin,
                IsVerified = true
            };

            modelBuilder.Entity<User>().HasData(admin);
        }
    }
}
