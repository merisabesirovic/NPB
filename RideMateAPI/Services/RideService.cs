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
	public class RideService
	{
		private readonly RideMateDbContext _db;
		private readonly NotificationService _notificationService;

		public RideService(RideMateDbContext db, NotificationService notificationService)
		{
			_db = db;
			_notificationService = notificationService;
		}

		public async Task<RideDto> CreateRideAsync(Guid driverId, CreateRideDto dto)
		{
			var departureDateTime = DateTimeHelper.AsUtc(dto.DepartureDateTime);
			var destinationDateTime = DateTimeHelper.AsUtc(dto.DestinationDateTime);

			// validate
			if (dto.AvailableSeats <= 0) throw new ArgumentException("AvailableSeats must be > 0");
			if (dto.PricePerSeat <= 0) throw new ArgumentException("PricePerSeat must be > 0");
			if (departureDateTime <= DateTime.UtcNow.AddMinutes(30)) throw new ArgumentException("Departure must be at least 30 minutes in the future");
			if (departureDateTime >= destinationDateTime) throw new ArgumentException("Destination time must be after departure time");
			var user = await GetVerifiedDriverAsync(driverId);
			await EnsureDriverHasNoOverlappingRideAsync(driverId, departureDateTime, destinationDateTime);

			var ride = new Ride
			{
				Id = Guid.NewGuid(),
				DriverId = driverId,
				StartAddress = dto.StartAddress,
				StartLatitude = dto.StartLatitude,
				StartLongitude = dto.StartLongitude,
				DestinationAddress = dto.DestinationAddress,
				DestinationLatitude = dto.DestinationLatitude,
				DestinationLongitude = dto.DestinationLongitude,
				DepartureDateTime = departureDateTime,
				DestinationDateTime = destinationDateTime,
				AvailableSeats = dto.AvailableSeats,
				PricePerSeat = dto.PricePerSeat,
				RideType = Enum.TryParse<RideType>(dto.RideType, out var rt) ? rt : RideType.OneTime,
				RideStatus = RideStatus.Scheduled,
				AutoApproveBookings = dto.AutoApproveBookings,
				SmokingAllowed = dto.SmokingAllowed,
				PetsAllowed = dto.PetsAllowed,
				LuggageAllowed = dto.LuggageAllowed,
				MusicAllowed = dto.MusicAllowed,
				ConversationAllowed = dto.ConversationAllowed,
				CreatedAt = DateTime.UtcNow,
				Driver = user
			};

			_db.Rides.Add(ride);
			await _db.SaveChangesAsync();

			return MapToDto(ride);
		}

		public async Task<RideDto> GetByIdAsync(Guid id)
		{
			var ride = await _db.Rides.Include(r => r.Driver).FirstOrDefaultAsync(r => r.Id == id);
			if (ride == null) return null;
			return MapToDto(ride);
		}

		public async Task<List<RideDto>> GetMyCreatedRidesAsync(Guid driverId)
		{
			await GetVerifiedDriverAsync(driverId);
			var rides = await _db.Rides
				.Include(r => r.Driver)
				.Where(r => r.DriverId == driverId && r.RideStatus != RideStatus.Cancelled)
				.ToListAsync();
			return rides.Select(r => MapToDto(r)).ToList();
		}

		public async Task<RideListResponse> SearchRidesAsync(DTOs.SearchRideRequest request, Guid? requestingUserId = null)
		{
			// Determine whether user asked for a specific departure date
			var hasDate = request.DepartureDate.HasValue;
			var searchDate = request.DepartureDate.HasValue ? DateTimeHelper.DateOnlyAsUtc(request.DepartureDate) : DateTime.UtcNow.Date;
			var today = DateTime.UtcNow.Date;
			if (hasDate && searchDate < today)
			{
				// cannot search for past dates
				return new RideListResponse { TotalCount = 0, Page = 1, PageSize = 0, Items = new List<RideDto>() };
			}

			// Start from rides that are visible in active listings.
			var q = _db.Rides
				.Include(r => r.Driver)
				.AsQueryable()
				.Where(r => r.RideStatus != RideStatus.Completed && r.RideStatus != RideStatus.Cancelled);
			if (requestingUserId.HasValue)
			{
				q = q.Where(r => r.DriverId != requestingUserId.Value);
			}

			if (!string.IsNullOrWhiteSpace(request.StartAddress))
			{
				var startAddress = request.StartAddress.Trim();
				q = q.Where(r => EF.Functions.ILike(r.StartAddress, $"%{startAddress}%"));
			}

			if (!string.IsNullOrWhiteSpace(request.DestinationAddress))
			{
				var destinationAddress = request.DestinationAddress.Trim();
				q = q.Where(r => EF.Functions.ILike(r.DestinationAddress, $"%{destinationAddress}%"));
			}

			// Location radius filtering: use great-circle distance calculation.
			// For PostgreSQL we can rely on client-side calculation only if coordinates provided.
			// Use simple bounding box first to leverage indexes, then precise Haversine filter.

			if (request.StartLatitude.HasValue && request.StartLongitude.HasValue && request.StartRadiusKm.HasValue)
			{
				// degrees approximation for ~latitude delta
				var lat = request.StartLatitude.Value;
				var lon = request.StartLongitude.Value;
				var radiusKm = request.StartRadiusKm.Value;

				// 1 deg latitude ~ 111 km
				var latDelta = radiusKm / 111.0;
				// longitude delta scaled by latitude
				var lonDelta = radiusKm / (111.0 * Math.Cos(lat * Math.PI / 180.0));

				var minLat = lat - latDelta;
				var maxLat = lat + latDelta;
				var minLon = lon - lonDelta;
				var maxLon = lon + lonDelta;

				q = q.Where(r => r.StartLatitude >= minLat && r.StartLatitude <= maxLat && r.StartLongitude >= minLon && r.StartLongitude <= maxLon);

				// precise filter using Haversine within LINQ-to-Objects after materialization is expensive; but we can compose expression to compute distance in SQL using raw formula if needed later.
			}

			if (request.DestinationLatitude.HasValue && request.DestinationLongitude.HasValue && request.DestinationRadiusKm.HasValue)
			{
				var lat = request.DestinationLatitude.Value;
				var lon = request.DestinationLongitude.Value;
				var radiusKm = request.DestinationRadiusKm.Value;
				var latDelta = radiusKm / 111.0;
				var lonDelta = radiusKm / (111.0 * Math.Cos(lat * Math.PI / 180.0));
				var minLat = lat - latDelta;
				var maxLat = lat + latDelta;
				var minLon = lon - lonDelta;
				var maxLon = lon + lonDelta;
				q = q.Where(r => r.DestinationLatitude >= minLat && r.DestinationLatitude <= maxLat && r.DestinationLongitude >= minLon && r.DestinationLongitude <= maxLon);
			}

			// Filter candidates by date and recurrence in the database as much as possible

			var now = DateTime.UtcNow;

			if (hasDate)
			{
				var start = searchDate.Date;
				var end = start.AddDays(1);
				var dow = searchDate.DayOfWeek;

				q = q.Where(r => (
							!r.IsRecurring && r.DepartureDateTime >= start && r.DepartureDateTime < end
						) || (
							r.IsRecurring && r.DepartureDateTime <= end && (r.RecurringEndDate == null || r.RecurringEndDate >= start) && (
								r.RecurringType == Models.RecurringType.Daily
								|| (r.RecurringType == Models.RecurringType.Weekdays && dow != DayOfWeek.Saturday && dow != DayOfWeek.Sunday)
								|| (r.RecurringType == Models.RecurringType.Weekends && (dow == DayOfWeek.Saturday || dow == DayOfWeek.Sunday))
							)
						)
					);
			}
			else
			{
				// No specific date requested: include any rides departing today or later (use date-based comparison to avoid timezone issues)
				var searchToday = now.Date;
				q = q.Where(r => (!r.IsRecurring && r.DepartureDateTime >= searchToday)
					|| (r.IsRecurring && (r.RecurringEndDate == null || r.RecurringEndDate >= searchToday)));
			}

			// Apply price filters at DB level when possible
			if (request.MinPrice.HasValue) q = q.Where(r => r.PricePerSeat >= request.MinPrice.Value);
			if (request.MaxPrice.HasValue) q = q.Where(r => r.PricePerSeat <= request.MaxPrice.Value);

			if (hasDate)
			{
				var start = searchDate.Date;
				var end = start.AddDays(1);
				var dow = searchDate.DayOfWeek;

				// Filter by occurrence on that date (one-time or recurring matching rules)
				q = q.Where(r => (
						!r.IsRecurring && r.DepartureDateTime >= start && r.DepartureDateTime < end
					) || (
						r.IsRecurring && r.DepartureDateTime <= end && (r.RecurringEndDate == null || r.RecurringEndDate >= start) && (
							r.RecurringType == Models.RecurringType.Daily
							|| (r.RecurringType == Models.RecurringType.Weekdays && dow != DayOfWeek.Saturday && dow != DayOfWeek.Sunday)
							|| (r.RecurringType == Models.RecurringType.Weekends && (dow == DayOfWeek.Saturday || dow == DayOfWeek.Sunday))
						)
					)
				);

				// Materialize candidates
				var candidates = await q.OrderBy(r => r.DepartureDateTime).ToListAsync();

				// Build occurrences and filter out past times for that date
				var validCandidates = new List<Ride>();
				foreach (var ride in candidates)
				{
					DateTime occurrence = ride.IsRecurring ? start.Add(ride.DepartureDateTime.TimeOfDay) : ride.DepartureDateTime;
					if (occurrence < now) continue;
					validCandidates.Add(ride);
				}

				if (!validCandidates.Any())
				{
					var emptyResp = new RideListResponse { TotalCount = 0, Page = request.Page, PageSize = request.PageSize, Items = new List<RideDto>() };
					if (request.Debug)
					{
						emptyResp.DebugInfo = new DTOs.SearchDebugInfo
						{
							DbCandidates = 0,
							ValidCandidates = 0,
							AvailableRides = 0,
							DateFilterApplied = true,
							PriceFilterApplied = request.MinPrice.HasValue || request.MaxPrice.HasValue
						};
					}
					return emptyResp;
				}

				// compute per-ride reserved seats for that date
				var rideIds = validCandidates.Select(r => r.Id).ToList();
				var reserved = await _db.Bookings.Where(b => rideIds.Contains(b.RideId) && b.RideDate.Date == start && b.BookingStatus == BookingStatus.Approved)
					.GroupBy(b => b.RideId)
					.Select(g => new { RideId = g.Key, Reserved = g.Sum(b => b.SeatsReserved) })
					.ToListAsync();
				var reservedLookup = reserved.ToDictionary(x => x.RideId, x => x.Reserved);
				var seatsLeftLookup = validCandidates.ToDictionary(
					r => r.Id,
					r => SeatsLeftForSearchDate(r, reservedLookup.TryGetValue(r.Id, out var reservedSeats) ? reservedSeats : 0));

				var availableRides = validCandidates.Where(r => seatsLeftLookup[r.Id] > 0).ToList();

				// Apply MinAvailableSeats filter if requested
				if (request.MinAvailableSeats.HasValue)
				{
					availableRides = availableRides.Where(r => seatsLeftLookup[r.Id] >= request.MinAvailableSeats.Value).ToList();
				}

				// paging
				var page = Math.Max(1, request.Page);
				var pageSize = Math.Clamp(request.PageSize, 1, 100);
				var total = availableRides.Count;
				var items = availableRides.Skip((page - 1) * pageSize).Take(pageSize).ToList();

				var resp = new RideListResponse
				{
					TotalCount = total,
					Page = page,
					PageSize = pageSize,
					Items = items.Select(r => MapToDto(r, seatsLeftLookup[r.Id])).ToList()
				};
				if (request.Debug)
				{
					resp.DebugInfo = new DTOs.SearchDebugInfo
					{
						DbCandidates = candidates.Count,
						ValidCandidates = validCandidates.Count,
						AvailableRides = availableRides.Count,
						DateFilterApplied = true,
						PriceFilterApplied = request.MinPrice.HasValue || request.MaxPrice.HasValue
					};
				}
				return resp;
			}
			else
			{
				// No date provided: include any future one-time rides and recurring rides that haven't ended
				q = q.Where(r => (!r.IsRecurring && r.DepartureDateTime >= now) || (r.IsRecurring && (r.RecurringEndDate == null || r.RecurringEndDate >= now.Date)));

				// Apply MinAvailableSeats filter at DB level if provided
				if (request.MinAvailableSeats.HasValue)
				{
					q = q.Where(r => r.AvailableSeats >= request.MinAvailableSeats.Value);
				}

				var candidates = await q.OrderBy(r => r.DepartureDateTime).ToListAsync();

				var availableRides = candidates.Where(r => r.AvailableSeats > 0).ToList();

				// paging
				var page = Math.Max(1, request.Page);
				var pageSize = Math.Clamp(request.PageSize, 1, 100);
				var total = availableRides.Count;
				var items = availableRides.Skip((page - 1) * pageSize).Take(pageSize).ToList();

				var resp = new RideListResponse
				{
					TotalCount = total,
					Page = page,
					PageSize = pageSize,
					Items = items.Select(r => MapToDto(r)).ToList()
				};
				if (request.Debug)
				{
					resp.DebugInfo = new DTOs.SearchDebugInfo
					{
						DbCandidates = candidates.Count,
						ValidCandidates = candidates.Count,
						AvailableRides = availableRides.Count,
						DateFilterApplied = false,
						PriceFilterApplied = request.MinPrice.HasValue || request.MaxPrice.HasValue
					};
				}
				return resp;
			}
		}

		public async Task<RideDto> UpdateRideAsync(Guid driverId, Guid rideId, UpdateRideDto dto)
		{
			var ride = await _db.Rides.FirstOrDefaultAsync(r => r.Id == rideId);
			if (ride == null) return null;
			if (ride.DriverId != driverId) throw new UnauthorizedAccessException();
			await GetVerifiedDriverAsync(driverId);
			if (ride.RideStatus == RideStatus.Completed || ride.RideStatus == RideStatus.Cancelled) throw new InvalidOperationException("Cannot modify completed or cancelled ride");

			if (dto.AvailableSeats.HasValue)
			{
				if (dto.AvailableSeats.Value <= 0) throw new ArgumentException("AvailableSeats must be > 0");
				ride.AvailableSeats = dto.AvailableSeats.Value;
			}
			if (dto.PricePerSeat.HasValue)
			{
				if (dto.PricePerSeat.Value <= 0) throw new ArgumentException("PricePerSeat must be > 0");
				ride.PricePerSeat = dto.PricePerSeat.Value;
			}

			if (!string.IsNullOrWhiteSpace(dto.StartAddress)) ride.StartAddress = dto.StartAddress;
			if (dto.StartLatitude.HasValue) ride.StartLatitude = dto.StartLatitude.Value;
			if (dto.StartLongitude.HasValue) ride.StartLongitude = dto.StartLongitude.Value;
			if (!string.IsNullOrWhiteSpace(dto.DestinationAddress)) ride.DestinationAddress = dto.DestinationAddress;
			if (dto.DestinationLatitude.HasValue) ride.DestinationLatitude = dto.DestinationLatitude.Value;
			if (dto.DestinationLongitude.HasValue) ride.DestinationLongitude = dto.DestinationLongitude.Value;
			if (dto.DepartureDateTime.HasValue) ride.DepartureDateTime = DateTimeHelper.AsUtc(dto.DepartureDateTime.Value);
			if (!string.IsNullOrWhiteSpace(dto.RideType) && Enum.TryParse<RideType>(dto.RideType, out var rt)) ride.RideType = rt;
			if (dto.AutoApproveBookings.HasValue) ride.AutoApproveBookings = dto.AutoApproveBookings.Value;
			if (dto.SmokingAllowed.HasValue) ride.SmokingAllowed = dto.SmokingAllowed.Value;
			if (dto.PetsAllowed.HasValue) ride.PetsAllowed = dto.PetsAllowed.Value;
			if (dto.LuggageAllowed.HasValue) ride.LuggageAllowed = dto.LuggageAllowed.Value;
			if (dto.MusicAllowed.HasValue) ride.MusicAllowed = dto.MusicAllowed.Value;
			if (dto.ConversationAllowed.HasValue) ride.ConversationAllowed = dto.ConversationAllowed.Value;

			await _db.SaveChangesAsync();
			return MapToDto(ride);
		}

		public async Task<bool> CancelRideAsync(Guid driverId, Guid rideId)
		{
			var ride = await _db.Rides
				.Include(r => r.Driver)
				.Include(r => r.Bookings)
					.ThenInclude(b => b.Payment)
				.Include(r => r.Bookings)
					.ThenInclude(b => b.Passenger)
				.FirstOrDefaultAsync(r => r.Id == rideId);
			if (ride == null) return false;
			if (ride.DriverId != driverId) throw new UnauthorizedAccessException();
			await GetVerifiedDriverAsync(driverId);

			if (ride.RideStatus == RideStatus.Cancelled) return true;

			var now = DateTime.UtcNow;
			ride.RideStatus = RideStatus.Cancelled;
			var activeBookings = ride.Bookings
				.Where(b => b.BookingStatus != BookingStatus.Cancelled
					&& b.BookingStatus != BookingStatus.Rejected
					&& b.BookingStatus != BookingStatus.Completed)
				.ToList();

			var notificationDrafts = new List<(Guid PassengerId, string Title, string Message)>();

			foreach (var booking in activeBookings)
			{
				var refund = CancellationRefundPolicy.Calculate(
					booking.Payment,
					CancellationRefundPolicy.BookingDepartureUtc(booking),
					CancellationActor.Driver,
					now);

				booking.BookingStatus = BookingStatus.Cancelled;
				CancellationRefundPolicy.ApplyToPayment(booking.Payment, refund, now);

				if (refund.RefundAmount > 0)
				{
					notificationDrafts.Add((
						booking.PassengerId,
						"Voznja je otkazana i refundirana",
						$"Vozac je otkazao voznju {RouteLabel(ride)}. Refundacija iznosi {FormatRsd(refund.RefundAmount)}. {refund.Description} Proverite svoj racun. BookingId: {booking.Id}"));
				}
				else
				{
					notificationDrafts.Add((
						booking.PassengerId,
						"Voznja je otkazana",
						$"Vozac je otkazao voznju {RouteLabel(ride)}. Rezervacija je otkazana. {refund.Description} BookingId: {booking.Id}"));
				}
			}

			if (activeBookings.Count > 0)
			{
				ApplyDriverCancellationPenalty(ride.Driver);
			}

			await _db.SaveChangesAsync();

			foreach (var notification in notificationDrafts)
			{
				await _notificationService.CreateNotificationAsync(notification.PassengerId, notification.Title, notification.Message);
			}

			return true;
		}

		public async Task<RideDto> ChangeStatusAsync(Guid driverId, Guid rideId, string status)
		{
			var ride = await _db.Rides
				.Include(r => r.Driver)
				.Include(r => r.Bookings)
					.ThenInclude(b => b.Passenger)
				.FirstOrDefaultAsync(r => r.Id == rideId);
			if (ride == null) return null;
			if (ride.DriverId != driverId) throw new UnauthorizedAccessException();
			await GetVerifiedDriverAsync(driverId);

			if (!Enum.TryParse<RideStatus>(status, true, out var parsed))
			{
				throw new ArgumentException("Invalid status value");
			}

			if (parsed == RideStatus.BookingOpen || parsed == RideStatus.BookingClosed)
			{
				throw new InvalidOperationException("Booking status is no longer used. Ride is available for booking as soon as it is created.");
			}

			if (ride.RideStatus == RideStatus.Completed && parsed != RideStatus.Completed)
			{
				throw new InvalidOperationException("Completed ride status cannot be changed");
			}

			if (ride.RideStatus == RideStatus.Cancelled && parsed != RideStatus.Cancelled)
			{
				throw new InvalidOperationException("Cancelled ride status cannot be changed");
			}

			if (parsed == RideStatus.Cancelled)
			{
				var cancelled = await CancelRideAsync(driverId, rideId);
				if (!cancelled) return null;
				var cancelledRide = await _db.Rides.Include(r => r.Driver).FirstOrDefaultAsync(r => r.Id == rideId);
				return cancelledRide == null ? null : MapToDto(cancelledRide);
			}

			if (parsed == ride.RideStatus)
			{
				return MapToDto(ride);
			}

			var now = DateTime.UtcNow;
			if (parsed == RideStatus.Scheduled)
			{
				if (!IsScheduledLikeStatus(ride.RideStatus))
				{
					throw new InvalidOperationException($"Transition from {ride.RideStatus} to {parsed} is not allowed");
				}
			}
			else if (parsed == RideStatus.InProgress)
			{
				if (!IsScheduledLikeStatus(ride.RideStatus))
				{
					throw new InvalidOperationException($"Transition from {ride.RideStatus} to {parsed} is not allowed");
				}

				if (now < DateTimeHelper.AsUtc(ride.DepartureDateTime))
				{
					throw new InvalidOperationException("Ride cannot be started before departure time");
				}
			}
			else if (parsed == RideStatus.Completed)
			{
				if (ride.RideStatus != RideStatus.InProgress)
				{
					throw new InvalidOperationException("Ride must be in progress before it can be completed");
				}

				if (now < DateTimeHelper.AsUtc(ride.DestinationDateTime))
				{
					throw new InvalidOperationException("Ride cannot be completed before destination time");
				}
			}

			var previousStatus = ride.RideStatus;
			var bookingsToComplete = parsed == RideStatus.Completed
				? ride.Bookings.Where(b => b.BookingStatus == BookingStatus.Approved).ToList()
				: new List<Booking>();
			ride.RideStatus = parsed;
			var notificationDrafts = BuildRideStatusNotifications(ride, parsed);

			if (parsed == RideStatus.Completed)
			{
				CompleteApprovedBookings(bookingsToComplete);
				if (previousStatus != RideStatus.Completed)
				{
					ApplyRideCompletionStats(ride.Driver, bookingsToComplete);
				}
			}

			await _db.SaveChangesAsync();

			foreach (var notification in notificationDrafts)
			{
				await _notificationService.CreateNotificationAsync(notification.PassengerId, notification.Title, notification.Message);
			}

			return MapToDto(ride);
		}

		private RideDto MapToDto(Ride ride, int? availableSeatsOverride = null)
		{
			return new RideDto
			{
				Id = ride.Id,
				DriverId = ride.DriverId,
				DriverEmail = ride.Driver?.Email,
				DriverIsVerified = ride.Driver != null && UserRoleHelper.HasApprovedIdentityVerification(ride.Driver),
				StartAddress = ride.StartAddress,
				StartLatitude = ride.StartLatitude,
				StartLongitude = ride.StartLongitude,
				DestinationAddress = ride.DestinationAddress,
				DestinationDateTime = ride.DestinationDateTime,
				DestinationLatitude = ride.DestinationLatitude,
				DestinationLongitude = ride.DestinationLongitude,
				DepartureDateTime = ride.DepartureDateTime,
				AvailableSeats = availableSeatsOverride ?? ride.AvailableSeats,
				PricePerSeat = ride.PricePerSeat,
				RideType = ride.RideType.ToString(),
				RideStatus = ride.RideStatus.ToString(),
				AutoApproveBookings = ride.AutoApproveBookings,
				SmokingAllowed = ride.SmokingAllowed,
				PetsAllowed = ride.PetsAllowed,
				LuggageAllowed = ride.LuggageAllowed,
				MusicAllowed = ride.MusicAllowed,
				ConversationAllowed = ride.ConversationAllowed,
				CreatedAt = ride.CreatedAt
			};
		}

		private static int SeatsLeftForSearchDate(Ride ride, int approvedReservedSeats)
		{
			if (!ride.IsRecurring)
			{
				return Math.Max(0, ride.AvailableSeats);
			}

			return Math.Max(0, ride.AvailableSeats - approvedReservedSeats);
		}

		private async Task<User> GetVerifiedDriverAsync(Guid driverId)
		{
			var user = await _db.Users
				.Include(u => u.DriverVerificationDocuments)
				.FirstOrDefaultAsync(u => u.Id == driverId);
			if (user == null) throw new ArgumentException("Driver not found");
			if (!UserRoleHelper.HasApprovedDriverVerification(user))
			{
				throw new InvalidOperationException("Driver profile is waiting for admin approval");
			}

			return user;
		}

		private async Task EnsureDriverHasNoOverlappingRideAsync(Guid driverId, DateTime departureDateTime, DateTime destinationDateTime)
		{
			var hasOverlap = await _db.Rides.AnyAsync(r =>
				r.DriverId == driverId
				&& r.RideStatus != RideStatus.Cancelled
				&& r.RideStatus != RideStatus.Completed
				&& r.DepartureDateTime < destinationDateTime
				&& departureDateTime < r.DestinationDateTime);

			if (hasOverlap)
			{
				throw new InvalidOperationException("Driver already has a ride in that time range");
			}
		}

		private static bool IsScheduledLikeStatus(RideStatus status)
		{
			return status == RideStatus.Scheduled
				|| status == RideStatus.BookingOpen
				|| status == RideStatus.BookingClosed;
		}

		private static List<(Guid PassengerId, string Title, string Message)> BuildRideStatusNotifications(Ride ride, RideStatus status)
		{
			var activePassengerBookings = ride.Bookings
				.Where(b => b.BookingStatus == BookingStatus.Approved)
				.ToList();

			var title = status switch
			{
				RideStatus.InProgress => "Voznja je krenula",
				RideStatus.Completed => "Voznja je zavrsena",
				_ => "Status voznje je promenjen"
			};

			return activePassengerBookings
				.Select(booking => (
					booking.PassengerId,
					title,
					StatusNotificationMessage(ride, booking, status)))
				.ToList();
		}

		private static string StatusNotificationMessage(Ride ride, Booking booking, RideStatus status)
		{
			return status switch
			{
				RideStatus.InProgress => $"Voznja {RouteLabel(ride)} je krenula. BookingId: {booking.Id}",
				RideStatus.Completed => $"Voznja {RouteLabel(ride)} je zavrsena. Mozete ostaviti recenziju. BookingId: {booking.Id}",
				_ => $"Status voznje {RouteLabel(ride)} je promenjen u {RideStatusLabel(status)}. BookingId: {booking.Id}"
			};
		}

		private static void CompleteApprovedBookings(List<Booking> bookings)
		{
			foreach (var booking in bookings)
			{
				booking.BookingStatus = BookingStatus.Completed;
			}
		}

		private static void ApplyRideCompletionStats(User? driver, List<Booking> completedBookings)
		{
			if (driver != null)
			{
				driver.CompletedRidesCount += 1;
				RecalculateTrustScore(driver);
			}

			foreach (var passenger in completedBookings.Select(b => b.Passenger).Where(p => p != null).DistinctBy(p => p.Id))
			{
				passenger.CompletedRidesCount += 1;
				RecalculateTrustScore(passenger);
			}
		}

		private static void RecalculateTrustScore(User user)
		{
			user.TrustScore = (user.AverageRating * 20.0) + (user.CompletedRidesCount * 0.2) - (user.CancelledRidesCount * 2.0);
		}

		private static string RouteLabel(Ride ride)
		{
			return $"{ride.StartAddress} - {ride.DestinationAddress}";
		}

		private static string RideStatusLabel(RideStatus status)
		{
			return status switch
			{
				RideStatus.Scheduled => "zakazano",
				RideStatus.InProgress => "u toku",
				RideStatus.Completed => "zavrseno",
				RideStatus.Cancelled => "otkazano",
				_ => status.ToString()
			};
		}

		private static string FormatRsd(decimal amount)
		{
			return $"{amount:N0} RSD";
		}

		private static void ApplyDriverCancellationPenalty(User driver)
		{
			if (driver == null) return;
			driver.CancelledRidesCount += 1;
			driver.TrustScore = (driver.AverageRating * 20.0) + (driver.CompletedRidesCount * 0.2) - (driver.CancelledRidesCount * 2.0);
		}
	}
}
