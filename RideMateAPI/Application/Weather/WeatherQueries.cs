using MediatR;

namespace RideMateAPI.Application.Weather
{
	public record GetWeatherForecastQuery() : IRequest<IEnumerable<WeatherForecast>>;

	public class GetWeatherForecastQueryHandler : IRequestHandler<GetWeatherForecastQuery, IEnumerable<WeatherForecast>>
	{
		private static readonly string[] Summaries =
		[
			"Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
		];

		public Task<IEnumerable<WeatherForecast>> Handle(GetWeatherForecastQuery request, CancellationToken cancellationToken)
		{
			var forecast = Enumerable.Range(1, 5).Select(index => new WeatherForecast
			{
				Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
				TemperatureC = Random.Shared.Next(-20, 55),
				Summary = Summaries[Random.Shared.Next(Summaries.Length)]
			})
			.ToArray()
			.AsEnumerable();

			return Task.FromResult(forecast);
		}
	}
}
