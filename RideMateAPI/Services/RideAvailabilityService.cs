using System;
using RideMateAPI.Models;

namespace RideMateAPI.Services
{
	public static class RideAvailabilityService
	{
		public static bool IsRideAvailableForDate(Ride ride, DateOnly date)
		{
			if (!ride.IsRecurring)
			{
				// non-recurring: check equality of departure date
				return DateOnly.FromDateTime(ride.DepartureDateTime) == date;
			}

			// recurring: check end date
			if (ride.RecurringEndDate.HasValue && date > DateOnly.FromDateTime(ride.RecurringEndDate.Value)) return false;

			switch (ride.RecurringType)
			{
				case Models.RecurringType.Daily:
					return date >= DateOnly.FromDateTime(ride.DepartureDateTime);
				case Models.RecurringType.Weekdays:
					return date >= DateOnly.FromDateTime(ride.DepartureDateTime) && date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday;
				case Models.RecurringType.Weekends:
					return date >= DateOnly.FromDateTime(ride.DepartureDateTime) && (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday);
				default:
					return false;
			}
		}
	}
}
