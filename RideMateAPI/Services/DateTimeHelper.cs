namespace RideMateAPI.Services
{
	public static class DateTimeHelper
	{
		public static DateTime UtcMinValue { get; } = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);

		public static DateTime AsUtc(DateTime value)
		{
			return value.Kind switch
			{
				DateTimeKind.Utc => value,
				DateTimeKind.Local => value.ToUniversalTime(),
				_ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
			};
		}

		public static DateTime? AsUtc(DateTime? value)
		{
			return value.HasValue ? AsUtc(value.Value) : null;
		}

		public static DateTime DateOnlyAsUtc(DateTime? value)
		{
			if (!value.HasValue) return UtcMinValue;
			return DateTime.SpecifyKind(value.Value.Date, DateTimeKind.Utc);
		}
	}
}
