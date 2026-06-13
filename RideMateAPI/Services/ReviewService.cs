using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RideMateAPI.Data;
using RideMateAPI.DTOs;
using RideMateAPI.Models;

namespace RideMateAPI.Services
{
	public class ReviewService
	{
		private readonly RideMateDbContext _db;

		public ReviewService(RideMateDbContext db)
		{
			_db = db;
		}

		public async Task<ReviewDto> CreateReviewAsync(Guid reviewerId, CreateReviewRequest req)
		{
			// Validate rating
			if (req.Rating < 1 || req.Rating > 5) throw new ArgumentException("Rating must be between 1 and 5");

			// Load booking and ensure reviewer participated
			var booking = await _db.Bookings.Include(b => b.Ride).FirstOrDefaultAsync(b => b.Id == req.BookingId);
			if (booking == null) throw new ArgumentException("Booking not found");

			// Ensure the booking is completed and the ride has finished before allowing a review
			if (booking.BookingStatus != BookingStatus.Completed || booking.Ride.RideStatus != RideStatus.Completed)
			{
				throw new InvalidOperationException("Reviews can only be left after the ride is completed");
			}

			if (booking.PassengerId != reviewerId && booking.Ride.DriverId != reviewerId)
			{
				throw new UnauthorizedAccessException("Reviewer did not participate in this booking");
			}

			// target user must match booking participants
			if (req.ReviewedUserId != booking.PassengerId && req.ReviewedUserId != booking.Ride.DriverId)
			{
				throw new ArgumentException("Reviewed user did not participate in this booking");
			}

			var existingReview = await _db.Reviews.AnyAsync(r => r.BookingId == booking.Id && r.ReviewerId == reviewerId);
			if (existingReview)
			{
				throw new InvalidOperationException("A review already exists for this booking");
			}

			var review = new Review
			{
				Id = Guid.NewGuid(),
				BookingId = booking.Id,
				ReviewerId = reviewerId,
				ReviewedUserId = req.ReviewedUserId,
				Rating = req.Rating,
				Comment = req.Comment,
				CreatedAt = DateTime.UtcNow
			};

			_db.Reviews.Add(review);

			// Update average rating and trust score for reviewed user
			var user = await _db.Users.Include(u => u.ReviewsReceived).FirstOrDefaultAsync(u => u.Id == req.ReviewedUserId);
			if (user == null) throw new ArgumentException("Reviewed user not found");

			await _db.SaveChangesAsync();

			await RecalculateTrustScoreAsync(req.ReviewedUserId);

			return MapToDto(await _db.Reviews
				.Include(r => r.Reviewer)
				.Include(r => r.ReviewedUser)
				.FirstAsync(r => r.Id == review.Id));
		}

		public async Task<List<ReviewDto>> GetUserReviewsAsync(Guid userId)
		{
			var reviews = await _db.Reviews
				.Include(r => r.Reviewer)
				.Include(r => r.ReviewedUser)
				.Where(r => r.ReviewedUserId == userId)
				.OrderByDescending(r => r.CreatedAt)
				.ToListAsync();
			return reviews.Select(MapToDto).ToList();
		}

		public async Task<List<ReviewDto>> GetMyReviewsAsync(Guid userId)
		{
			var reviews = await _db.Reviews
				.Include(r => r.Reviewer)
				.Include(r => r.ReviewedUser)
				.Where(r => r.ReviewedUserId == userId)
				.OrderByDescending(r => r.CreatedAt)
				.ToListAsync();
			return reviews.Select(MapToDto).ToList();
		}

		public async Task<List<ReviewDto>> GetMyWrittenReviewsAsync(Guid userId)
		{
			var reviews = await _db.Reviews
				.Include(r => r.Reviewer)
				.Include(r => r.ReviewedUser)
				.Where(r => r.ReviewerId == userId)
				.OrderByDescending(r => r.CreatedAt)
				.ToListAsync();
			return reviews.Select(MapToDto).ToList();
		}

		public async Task RecalculateTrustScoreAsync(Guid userId)
		{
			var user = await _db.Users.Include(u => u.ReviewsReceived).FirstOrDefaultAsync(u => u.Id == userId);
			if (user == null) return;

			var ratings = await _db.Reviews.Where(r => r.ReviewedUserId == userId).ToListAsync();
			if (ratings.Count > 0)
			{
				user.AverageRating = Math.Round(ratings.Average(r => r.Rating), 2);
			}
			else
			{
				user.AverageRating = 0;
			}

			user.TrustScore = (user.AverageRating * 20.0) + (user.CompletedRidesCount * 0.2) - (user.CancelledRidesCount * 2.0);

			await _db.SaveChangesAsync();
		}

		private ReviewDto MapToDto(Review r)
		{
			return new ReviewDto
			{
				Id = r.Id,
				BookingId = r.BookingId,
				ReviewerId = r.ReviewerId,
				ReviewedUserId = r.ReviewedUserId,
				ReviewerEmail = r.Reviewer?.Email ?? string.Empty,
				ReviewerFirstName = r.Reviewer?.FirstName ?? string.Empty,
				ReviewerLastName = r.Reviewer?.LastName ?? string.Empty,
				ReviewerIsVerified = r.Reviewer != null && UserRoleHelper.HasApprovedIdentityVerification(r.Reviewer),
				ReviewedUserEmail = r.ReviewedUser?.Email ?? string.Empty,
				ReviewedUserFirstName = r.ReviewedUser?.FirstName ?? string.Empty,
				ReviewedUserLastName = r.ReviewedUser?.LastName ?? string.Empty,
				ReviewedUserIsVerified = r.ReviewedUser != null && UserRoleHelper.HasApprovedIdentityVerification(r.ReviewedUser),
				Rating = r.Rating,
				Comment = r.Comment,
				CreatedAt = r.CreatedAt
			};
		}
	}
}
