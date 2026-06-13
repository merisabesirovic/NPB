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
	public class DisputeService
	{
		private readonly RideMateDbContext _db;

		public DisputeService(RideMateDbContext db)
		{
			_db = db;
		}

		public async Task<DisputeDto> CreateDisputeAsync(Guid userId, CreateDisputeRequest req)
		{
			var booking = await _db.Bookings.Include(b => b.Ride).FirstOrDefaultAsync(b => b.Id == req.BookingId);
			if (booking == null) throw new ArgumentException("Booking not found");

			// Only participants may open dispute
			if (booking.PassengerId != userId && booking.Ride.DriverId != userId) throw new UnauthorizedAccessException();

			var existingDispute = await _db.Disputes.AnyAsync(d => d.BookingId == booking.Id);
			if (existingDispute) throw new InvalidOperationException("A dispute already exists for this booking");

			var dispute = new Dispute
			{
				Id = Guid.NewGuid(),
				BookingId = booking.Id,
				CreatedByUserId = userId,
				Description = req.Description,
				Status = DisputeStatus.Open,
				Resolution = string.Empty,
				CreatedAt = DateTime.UtcNow
			};

			_db.Disputes.Add(dispute);
			await _db.SaveChangesAsync();

			return MapToDto(await _db.Disputes
				.Include(d => d.CreatedByUser)
				.Include(d => d.Booking).ThenInclude(b => b.Ride)
				.FirstAsync(d => d.Id == dispute.Id));
		}

		public async Task<List<DisputeDto>> GetMyDisputesAsync(Guid userId)
		{
			var disputes = await _db.Disputes
				.Include(d => d.CreatedByUser)
				.Include(d => d.Booking).ThenInclude(b => b.Ride)
				.Where(d => d.CreatedByUserId == userId || d.Booking.PassengerId == userId || d.Booking.Ride.DriverId == userId)
				.OrderByDescending(d => d.CreatedAt)
				.ToListAsync();
			return disputes.Select(MapToDto).ToList();
		}

		public async Task<DisputeDto> GetDisputeDetailsAsync(Guid userId, Guid disputeId)
		{
			var dispute = await _db.Disputes
				.Include(d => d.CreatedByUser)
				.Include(d => d.Booking).ThenInclude(b => b.Ride)
				.FirstOrDefaultAsync(d => d.Id == disputeId);
			if (dispute == null) return null;
			if (dispute.CreatedByUserId != userId && dispute.Booking.PassengerId != userId && dispute.Booking.Ride.DriverId != userId) throw new UnauthorizedAccessException();
			return MapToDto(dispute);
		}

		public async Task<List<DisputeDto>> AdminGetAllAsync()
		{
			var list = await _db.Disputes
				.Include(d => d.CreatedByUser)
				.Include(d => d.Booking).ThenInclude(b => b.Ride)
				.OrderByDescending(d => d.CreatedAt)
				.ToListAsync();
			return list.Select(MapToDto).ToList();
		}

		public async Task<DisputeDto> ChangeStatusAsync(Guid adminId, Guid disputeId, ChangeDisputeStatusRequest req)
		{
			var dispute = await _db.Disputes
				.Include(d => d.CreatedByUser)
				.Include(d => d.Booking).ThenInclude(b => b.Ride)
				.FirstOrDefaultAsync(d => d.Id == disputeId);
			if (dispute == null) return null;

			if (!Enum.TryParse<DisputeStatus>(req.Status, true, out var parsed)) throw new ArgumentException("Invalid status");

			dispute.Status = parsed;
			dispute.Resolution = req.Resolution;
			await _db.SaveChangesAsync();
			return MapToDto(dispute);
		}

		private DisputeDto MapToDto(Dispute d)
		{
			return new DisputeDto
			{
				Id = d.Id,
				BookingId = d.BookingId,
				CreatedByUserId = d.CreatedByUserId,
				CreatedByEmail = d.CreatedByUser?.Email ?? string.Empty,
				CreatedByFirstName = d.CreatedByUser?.FirstName ?? string.Empty,
				CreatedByLastName = d.CreatedByUser?.LastName ?? string.Empty,
				CreatedByIsVerified = d.CreatedByUser != null && UserRoleHelper.HasApprovedIdentityVerification(d.CreatedByUser),
				StartAddress = d.Booking?.Ride?.StartAddress ?? string.Empty,
				DestinationAddress = d.Booking?.Ride?.DestinationAddress ?? string.Empty,
				Description = d.Description,
				Status = d.Status.ToString(),
				Resolution = d.Resolution,
				CreatedAt = d.CreatedAt
			};
		}
	}
}
