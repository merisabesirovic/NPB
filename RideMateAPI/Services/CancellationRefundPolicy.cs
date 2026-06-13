using RideMateAPI.Models;

namespace RideMateAPI.Services
{
	public enum CancellationActor
	{
		Passenger,
		Driver
	}

	public sealed record RefundDecision(
		bool HasRefundablePayment,
		decimal RefundAmount,
		decimal RefundPercent,
		string Description);

	public static class CancellationRefundPolicy
	{
		public const double FullRefundHoursBeforeDeparture = 24;
		public const decimal PassengerPartialRefundPercent = 0.5m;

		public static RefundDecision Calculate(Payment? payment, DateTime departureUtc, CancellationActor actor, DateTime nowUtc)
		{
			if (payment == null || payment.PaymentMethod != PaymentMethod.Online || payment.PaymentStatus != PaymentStatus.Paid)
			{
				return new RefundDecision(false, 0m, 0m, "Nema online uplate za refundaciju.");
			}

			if (actor == CancellationActor.Driver)
			{
				return new RefundDecision(true, payment.Amount, 1m, "Vozac je otkazao, zato se vraca pun iznos.");
			}

			var normalizedDeparture = DateTimeHelper.AsUtc(departureUtc);
			var normalizedNow = DateTimeHelper.AsUtc(nowUtc);

			if (normalizedNow >= normalizedDeparture)
			{
				return new RefundDecision(true, 0m, 0m, "Nakon polaska nema refundacije.");
			}

			var hoursToDeparture = (normalizedDeparture - normalizedNow).TotalHours;
			if (hoursToDeparture >= FullRefundHoursBeforeDeparture)
			{
				return new RefundDecision(true, payment.Amount, 1m, "Otkazano je najmanje 24h pre polaska, zato se vraca pun iznos.");
			}

			var partialRefund = Math.Round(payment.Amount * PassengerPartialRefundPercent, 2);
			return new RefundDecision(true, partialRefund, PassengerPartialRefundPercent, "Otkazano je manje od 24h pre polaska, zato se vraca 50% iznosa.");
		}

		public static DateTime BookingDepartureUtc(Booking booking)
		{
			var ride = booking.Ride;
			var departure = ride.IsRecurring
				? booking.RideDate.Date.Add(ride.DepartureDateTime.TimeOfDay)
				: ride.DepartureDateTime;

			return DateTimeHelper.AsUtc(departure);
		}

		public static void ApplyToPayment(Payment? payment, RefundDecision decision, DateTime nowUtc)
		{
			if (!decision.HasRefundablePayment || payment == null)
			{
				return;
			}

			payment.RefundAmount = decision.RefundAmount;

			if (decision.RefundAmount > 0)
			{
				payment.PaymentStatus = PaymentStatus.Refunded;
				payment.RefundedAt = DateTimeHelper.AsUtc(nowUtc);
			}
		}
	}
}
