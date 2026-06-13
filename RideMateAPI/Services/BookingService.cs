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
	public class BookingService
	{
		private readonly RideMateDbContext _db;
		private readonly NotificationService _notificationService;

		public BookingService(RideMateDbContext db, NotificationService notificationService)
		{
			_db = db;
			_notificationService = notificationService;
		}

		public async Task<BookingDto> CreateBookingAsync(Guid passengerId, CreateBookingRequest req)
		{
			var ride = await _db.Rides.Include(r => r.Driver).FirstOrDefaultAsync(r => r.Id == req.RideId);
			if (ride == null) throw new ArgumentException("Ride not found");
			if (ride.DriverId == passengerId) throw new InvalidOperationException("Drivers cannot book their own rides");
			// check ride availability for the requested date
			// Determine booking date: for recurring rides passenger must provide RideDate; for one-time rides use the ride's departure date
			DateTime bookingDate;
			if (ride.IsRecurring)
			{
				if (req.RideDate == null) throw new ArgumentException("RideDate is required for recurring rides");
				bookingDate = DateTimeHelper.DateOnlyAsUtc(req.RideDate);
				if (!RideAvailabilityService.IsRideAvailableForDate(ride, DateOnly.FromDateTime(bookingDate)))
				{
					throw new InvalidOperationException("Ride is not available on requested date");
				}
			}
			else
			{
				// one-time ride: use ride's departure date
				bookingDate = DateTimeHelper.DateOnlyAsUtc(ride.DepartureDateTime);
			}
			if (req.SeatsReserved <= 0) throw new ArgumentException("SeatsReserved must be > 0");
			if (!IsRideReservable(ride.RideStatus)) throw new InvalidOperationException("Ride not open for booking");

			var existingActiveBooking = await _db.Bookings
				.FirstOrDefaultAsync(b => b.RideId == ride.Id
					&& b.PassengerId == passengerId
					&& b.RideDate.Date == bookingDate
					&& b.BookingStatus != BookingStatus.Cancelled
					&& b.BookingStatus != BookingStatus.Rejected
					&& b.BookingStatus != BookingStatus.Completed);
			if (existingActiveBooking != null)
			{
				throw new InvalidOperationException("You already have an active booking for this ride");
			}

			var seatsLeftForDate = await GetSeatsLeftForDateAsync(ride, bookingDate);
			if (seatsLeftForDate < req.SeatsReserved) throw new InvalidOperationException("Not enough available seats for selected date");

			var booking = new Booking
			{
				Id = Guid.NewGuid(),
				RideId = ride.Id,
				RideDate = bookingDate,
				PassengerId = passengerId,
				SeatsReserved = req.SeatsReserved,
				PickupPoint = req.PickupPoint,
				DropoffPoint = req.DropoffPoint,
				Note = req.Note ?? string.Empty,
				BookingStatus = BookingStatus.Pending,
				TotalPrice = ride.PricePerSeat * req.SeatsReserved,
				CreatedAt = DateTime.UtcNow
			};

			// handle payment mock
			Payment payment = null;
			if (!string.IsNullOrWhiteSpace(req.PaymentMethod))
			{
				if (!Enum.TryParse<PaymentMethod>(req.PaymentMethod, true, out var pm))
				{
					throw new ArgumentException("Invalid payment method");
				}
				payment = new Payment
				{
					Id = Guid.NewGuid(),
					Amount = 0m,
					PaymentMethod = pm,
					PaymentStatus = PaymentStatus.Pending,
					CreatedAt = DateTime.UtcNow
				};
				// link payment to booking (EF will fix FKs when tracked)
				payment.BookingId = booking.Id;
				booking.Payment = payment;
			}

			_db.Bookings.Add(booking);
			if (payment != null) _db.Payments.Add(payment);

			// If ride has AutoApproveBookings, try to approve immediately within a transaction
			if (ride.AutoApproveBookings)
			{
				using var tx = await _db.Database.BeginTransactionAsync();
				try
				{
					var rideForUpdate = await _db.Rides.FirstOrDefaultAsync(r => r.Id == ride.Id);
					if (rideForUpdate == null) throw new ArgumentException("Ride not found");
					var seatsLeftForUpdate = await GetSeatsLeftForDateAsync(rideForUpdate, bookingDate);
					if (seatsLeftForUpdate < req.SeatsReserved) throw new InvalidOperationException("Not enough available seats");
					if (!rideForUpdate.IsRecurring)
					{
						rideForUpdate.AvailableSeats -= req.SeatsReserved;
					}
					booking.BookingStatus = BookingStatus.Approved;
					await _db.SaveChangesAsync();
					await tx.CommitAsync();
					await _notificationService.CreateNotificationAsync(
						booking.PassengerId,
						"Rezervacija je odobrena",
						$"Vozac je automatski odobrio rezervaciju za {RouteLabel(ride)}. BookingId: {booking.Id}");
				}
				catch
				{
					await tx.RollbackAsync();
					throw;
				}
			}
			else
			{
				await _db.SaveChangesAsync();
				var passenger = await _db.Users.FirstOrDefaultAsync(u => u.Id == passengerId);
				await _notificationService.CreateNotificationAsync(
					ride.DriverId,
					"Novi zahtev za rezervaciju",
					$"Putnik {UserLabel(passenger)} zeli da rezervise {booking.SeatsReserved} sedista za {RouteLabel(ride)}. BookingId: {booking.Id}");
			}

			return MapToDto(await LoadBookingAsync(booking.Id));
		}

		public async Task<BookingDto> UpdateBookingAsync(Guid passengerId, Guid bookingId, UpdateBookingRequest req)
		{
			var booking = await LoadBookingAsync(bookingId);
			if (booking == null) return null;
			if (booking.PassengerId != passengerId) throw new UnauthorizedAccessException();
			if (booking.BookingStatus == BookingStatus.Cancelled || booking.BookingStatus == BookingStatus.Rejected || booking.BookingStatus == BookingStatus.Completed)
			{
				throw new InvalidOperationException("This booking cannot be changed");
			}

			booking.Note = req.Note ?? string.Empty;

			await _db.SaveChangesAsync();
			await _notificationService.CreateNotificationAsync(
				booking.Ride.DriverId,
				"Napomena je azurirana",
				$"Putnik {UserLabel(booking.Passenger)} je dodao napomenu za {RouteLabel(booking.Ride)}. BookingId: {booking.Id}");

			return MapToDto(booking);
		}

		public async Task<BookingDto> PayOnlineAsync(Guid passengerId, Guid bookingId, MockOnlinePaymentRequest req)
		{
			var booking = await LoadBookingAsync(bookingId);
			if (booking == null) return null;
			if (booking.PassengerId != passengerId) throw new UnauthorizedAccessException();
			if (booking.BookingStatus != BookingStatus.Approved) throw new InvalidOperationException("Only approved bookings can be paid online");
			if (string.IsNullOrWhiteSpace(req.CardholderName) || string.IsNullOrWhiteSpace(req.CardNumber) || string.IsNullOrWhiteSpace(req.Expiry) || string.IsNullOrWhiteSpace(req.Cvv))
			{
				throw new ArgumentException("Card data is required");
			}

			if (booking.Payment == null)
			{
				booking.Payment = new Payment
				{
					Id = Guid.NewGuid(),
					BookingId = booking.Id,
					Amount = booking.TotalPrice,
					PaymentMethod = PaymentMethod.Online,
					PaymentStatus = PaymentStatus.Paid,
					CreatedAt = DateTime.UtcNow
				};
				_db.Payments.Add(booking.Payment);
			}
			else
			{
				booking.Payment.PaymentMethod = PaymentMethod.Online;
				booking.Payment.PaymentStatus = PaymentStatus.Paid;
				booking.Payment.Amount = booking.TotalPrice;
			}

			await _db.SaveChangesAsync();
			await _notificationService.CreateNotificationAsync(
				booking.Ride.DriverId,
				"Rezervacija je placena",
				$"Putnik {UserLabel(booking.Passenger)} je platio online za {RouteLabel(booking.Ride)}. BookingId: {booking.Id}");

			return MapToDto(booking);
		}

		public async Task<BookingDto> MarkCashPaidAsync(Guid driverId, Guid bookingId)
		{
			await EnsureVerifiedDriverAsync(driverId);
			var booking = await _db.Bookings.Include(b => b.Ride).ThenInclude(r => r.Driver).Include(b => b.Passenger).Include(b => b.Payment).FirstOrDefaultAsync(b => b.Id == bookingId);
			if (booking == null) return null;
			if (booking.Ride.DriverId != driverId) throw new UnauthorizedAccessException();
			if (booking.BookingStatus != BookingStatus.Approved) throw new InvalidOperationException("Only approved bookings can be marked paid");
			if (booking.Payment != null && booking.Payment.PaymentMethod == PaymentMethod.Online)
			{
				throw new InvalidOperationException("Online bookings must be paid by passenger");
			}

			if (booking.Payment == null)
			{
				// create payment record as cash paid
				var payment = new Payment
				{
					Id = Guid.NewGuid(),
					BookingId = booking.Id,
					Amount = booking.TotalPrice,
					PaymentMethod = PaymentMethod.Cash,
					PaymentStatus = PaymentStatus.Paid,
					CreatedAt = DateTime.UtcNow
				};
				booking.Payment = payment;
				_db.Payments.Add(payment);
			}
			else
			{
				// if existing payment pending, mark paid
				booking.Payment.PaymentStatus = PaymentStatus.Paid;
				booking.Payment.Amount = booking.TotalPrice;
				booking.Payment.PaymentMethod = PaymentMethod.Cash;
			}

			await _db.SaveChangesAsync();
			return MapToDto(booking);
		}

		public async Task<BookingDto> ApproveBookingAsync(Guid driverId, Guid bookingId)
		{
			await EnsureVerifiedDriverAsync(driverId);
			// Use transaction to avoid overbooking
			using var tx = await _db.Database.BeginTransactionAsync();
			try
			{
				var booking = await _db.Bookings.Include(b => b.Ride).ThenInclude(r => r.Driver).Include(b => b.Passenger).Include(b => b.Payment).FirstOrDefaultAsync(b => b.Id == bookingId);
				if (booking == null) return null;
				if (booking.Ride.DriverId != driverId) throw new UnauthorizedAccessException();
				if (booking.BookingStatus != BookingStatus.Pending) throw new InvalidOperationException("Only pending bookings can be approved");

				var ride = await _db.Rides.FirstOrDefaultAsync(r => r.Id == booking.RideId);
				if (ride == null) throw new ArgumentException("Ride not found");
				var seatsLeftForDate = await GetSeatsLeftForDateAsync(ride, booking.RideDate);
				if (seatsLeftForDate < booking.SeatsReserved) throw new InvalidOperationException("Not enough seats to approve booking for that date");

				if (!ride.IsRecurring)
				{
					ride.AvailableSeats -= booking.SeatsReserved;
				}
				booking.BookingStatus = BookingStatus.Approved;

				await _db.SaveChangesAsync();
				await tx.CommitAsync();

				await _notificationService.CreateNotificationAsync(
					booking.PassengerId,
					"Rezervacija je odobrena",
					$"Vozac je odobrio rezervaciju za {RouteLabel(booking.Ride)}. BookingId: {booking.Id}");
				// if payment was cash and driver needs to record payment later, nothing to do; if payment was pending online, no change

				return MapToDto(booking);
			}
			catch
			{
				await tx.RollbackAsync();
				throw;
			}
		}

		public async Task<BookingDto> RejectBookingAsync(Guid driverId, Guid bookingId)
		{
			await EnsureVerifiedDriverAsync(driverId);
			var booking = await _db.Bookings.Include(b => b.Ride).ThenInclude(r => r.Driver).Include(b => b.Passenger).Include(b => b.Payment).FirstOrDefaultAsync(b => b.Id == bookingId);
			if (booking == null) return null;
			if (booking.Ride.DriverId != driverId) throw new UnauthorizedAccessException();
			if (booking.BookingStatus != BookingStatus.Pending) throw new InvalidOperationException("Only pending bookings can be rejected");

			booking.BookingStatus = BookingStatus.Rejected;
			await _db.SaveChangesAsync();

			await _notificationService.CreateNotificationAsync(
				booking.PassengerId,
				"Rezervacija je odbijena",
				$"Vozac je odbio rezervaciju za {RouteLabel(booking.Ride)}. BookingId: {booking.Id}");
			return MapToDto(booking);
		}

		public async Task<BookingDto> CancelBookingAsync(Guid userId, Guid bookingId)
		{
			var booking = await _db.Bookings.Include(b => b.Ride).ThenInclude(r => r.Driver).Include(b => b.Passenger).Include(b => b.Payment).FirstOrDefaultAsync(b => b.Id == bookingId);
			if (booking == null) return null;

			// passenger can cancel their booking, driver can also cancel
			if (booking.PassengerId != userId && booking.Ride.DriverId != userId) throw new UnauthorizedAccessException();

			if (booking.BookingStatus == BookingStatus.Cancelled)
			{
				return MapToDto(booking);
			}

			var cancelledByPassenger = booking.PassengerId == userId;
			var wasApproved = booking.BookingStatus == BookingStatus.Approved;
			if (cancelledByPassenger && IsRideStartedOrCompleted(booking.Ride.RideStatus))
			{
				throw new InvalidOperationException("Passenger bookings cannot be cancelled after ride has started");
			}

			if (cancelledByPassenger && booking.BookingStatus == BookingStatus.Completed)
			{
				throw new InvalidOperationException("Completed bookings cannot be cancelled");
			}

			var now = DateTime.UtcNow;
			var refund = CancellationRefundPolicy.Calculate(
				booking.Payment,
				CancellationRefundPolicy.BookingDepartureUtc(booking),
				cancelledByPassenger ? CancellationActor.Passenger : CancellationActor.Driver,
				now);

			booking.BookingStatus = BookingStatus.Cancelled;
			if (wasApproved && !booking.Ride.IsRecurring)
			{
				booking.Ride.AvailableSeats += booking.SeatsReserved;
			}
			CancellationRefundPolicy.ApplyToPayment(booking.Payment, refund, now);

			if (cancelledByPassenger)
			{
				ApplyCancellationPenalty(booking.Passenger);
			}
			else
			{
				ApplyCancellationPenalty(booking.Ride.Driver);
			}

			await _db.SaveChangesAsync();

			if (cancelledByPassenger)
			{
				await _notificationService.CreateNotificationAsync(
					booking.Ride.DriverId,
					"Rezervacija je otkazana",
					$"Putnik {UserLabel(booking.Passenger)} je otkazao rezervaciju za {RouteLabel(booking.Ride)}. BookingId: {booking.Id}");

				if (refund.HasRefundablePayment)
				{
					await _notificationService.CreateNotificationAsync(
						booking.PassengerId,
						refund.RefundAmount > 0 ? "Rezervacija je otkazana i refundirana" : "Rezervacija je otkazana",
						$"Otkazali ste rezervaciju za {RouteLabel(booking.Ride)}. {RefundMessage(refund)} BookingId: {booking.Id}");
				}
			}
			else
			{
				await _notificationService.CreateNotificationAsync(
					booking.PassengerId,
					refund.RefundAmount > 0 ? "Rezervacija je otkazana i refundirana" : "Rezervacija je otkazana",
					$"Vozac je otkazao rezervaciju za {RouteLabel(booking.Ride)}. {RefundMessage(refund)} BookingId: {booking.Id}");
			}

			return MapToDto(booking);
		}

		public async Task<List<BookingDto>> GetMyBookingsAsync(Guid userId)
		{
			var bookings = await _db.Bookings
				.Include(b => b.Ride).ThenInclude(r => r.Driver)
				.Include(b => b.Passenger)
				.Include(b => b.Payment)
				.Where(b => b.PassengerId == userId)
				.OrderByDescending(b => b.CreatedAt)
				.ToListAsync();
			return bookings.Select(MapToDto).ToList();
		}

		public async Task<List<BookingDto>> GetDriverBookingsAsync(Guid driverId)
		{
			await EnsureVerifiedDriverAsync(driverId);
			var bookings = await _db.Bookings
				.Include(b => b.Ride).ThenInclude(r => r.Driver)
				.Include(b => b.Passenger)
				.Include(b => b.Payment)
				.Where(b => b.Ride.DriverId == driverId)
				.OrderByDescending(b => b.CreatedAt)
				.ToListAsync();
			return bookings.Select(MapToDto).ToList();
		}

		private BookingDto MapToDto(Booking b)
		{
			return new BookingDto
			{
				Id = b.Id,
				RideId = b.RideId,
				PassengerId = b.PassengerId,
				SeatsReserved = b.SeatsReserved,
				PickupPoint = b.PickupPoint,
				DropoffPoint = b.DropoffPoint,
				Note = b.Note,
				BookingStatus = b.BookingStatus.ToString(),
				TotalPrice = b.TotalPrice,
				CreatedAt = b.CreatedAt,
				RideDate = b.RideDate,
				PaymentMethod = b.Payment?.PaymentMethod.ToString(),
				PaymentStatus = b.Payment?.PaymentStatus.ToString(),
				PaidAmount = b.Payment?.Amount,
				RefundAmount = b.Payment?.RefundAmount,
				DriverId = b.Ride?.DriverId ?? Guid.Empty,
				DriverEmail = b.Ride?.Driver?.Email ?? string.Empty,
				DriverFirstName = b.Ride?.Driver?.FirstName ?? string.Empty,
				DriverLastName = b.Ride?.Driver?.LastName ?? string.Empty,
				DriverIsVerified = b.Ride?.Driver != null && UserRoleHelper.HasApprovedIdentityVerification(b.Ride.Driver),
				PassengerEmail = b.Passenger?.Email ?? string.Empty,
				PassengerFirstName = b.Passenger?.FirstName ?? string.Empty,
				PassengerLastName = b.Passenger?.LastName ?? string.Empty,
				PassengerIsVerified = b.Passenger != null && UserRoleHelper.HasApprovedIdentityVerification(b.Passenger),
				StartAddress = b.Ride?.StartAddress ?? string.Empty,
				DestinationAddress = b.Ride?.DestinationAddress ?? string.Empty,
				DepartureDateTime = b.Ride?.DepartureDateTime ?? default,
				RideStatus = b.Ride?.RideStatus.ToString() ?? string.Empty
			};
		}

		private async Task<Booking> LoadBookingAsync(Guid bookingId)
		{
			return await _db.Bookings
				.Include(b => b.Ride).ThenInclude(r => r.Driver)
				.Include(b => b.Passenger)
				.Include(b => b.Payment)
				.FirstOrDefaultAsync(b => b.Id == bookingId);
		}

		private async Task EnsureVerifiedDriverAsync(Guid driverId)
		{
			var isVerifiedDriver = await _db.Users.AnyAsync(u =>
				u.Id == driverId
				&& (u.Roles & UserRole.Driver) == UserRole.Driver
				&& u.DriverVerificationDocuments.Any(d => d.DocumentType != DocumentType.PassengerIdentityCard && d.VerificationStatus == VerificationStatus.Approved));

			if (!isVerifiedDriver)
			{
				throw new InvalidOperationException("Driver profile is waiting for admin approval");
			}
		}

		private async Task<int> GetSeatsLeftForDateAsync(Ride ride, DateTime bookingDate)
		{
			if (!ride.IsRecurring)
			{
				return Math.Max(0, ride.AvailableSeats);
			}

			var existingReserved = await _db.Bookings
				.Where(b => b.RideId == ride.Id
					&& b.RideDate.Date == bookingDate.Date
					&& b.BookingStatus == BookingStatus.Approved)
				.SumAsync(b => (int?)b.SeatsReserved) ?? 0;

			return Math.Max(0, ride.AvailableSeats - existingReserved);
		}

		private static string UserLabel(User user)
		{
			if (user == null) return "korisnik";
			var name = $"{user.FirstName} {user.LastName}".Trim();
			return string.IsNullOrWhiteSpace(name) ? user.Email : name;
		}

		private static string RouteLabel(Ride ride)
		{
			return ride == null ? "voznju" : $"{ride.StartAddress} - {ride.DestinationAddress}";
		}

		private static bool IsRideReservable(RideStatus status)
		{
			return status == RideStatus.Scheduled
				|| status == RideStatus.BookingOpen
				|| status == RideStatus.BookingClosed;
		}

		private static bool IsRideStartedOrCompleted(RideStatus status)
		{
			return status == RideStatus.InProgress || status == RideStatus.Completed;
		}

		private static string RefundMessage(RefundDecision refund)
		{
			if (!refund.HasRefundablePayment)
			{
				return refund.Description;
			}

			if (refund.RefundAmount > 0)
			{
				return $"Refundacija iznosi {FormatRsd(refund.RefundAmount)}. {refund.Description} Proverite svoj racun.";
			}

			return $"Refundacija iznosi {FormatRsd(0m)}. {refund.Description}";
		}

		private static string FormatRsd(decimal amount)
		{
			return $"{amount:N0} RSD";
		}

		private static void ApplyCancellationPenalty(User user)
		{
			if (user == null) return;
			user.CancelledRidesCount += 1;
			user.TrustScore = (user.AverageRating * 20.0) + (user.CompletedRidesCount * 0.2) - (user.CancelledRidesCount * 2.0);
		}

	}
}
