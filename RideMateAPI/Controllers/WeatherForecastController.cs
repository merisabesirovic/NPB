using MediatR;
using Microsoft.AspNetCore.Mvc;
using RideMateAPI.Application.Weather;

namespace RideMateAPI.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class WeatherForecastController : ControllerBase
	{
		private readonly IMediator _mediator;

		public WeatherForecastController(IMediator mediator)
		{
			_mediator = mediator;
		}

		[HttpGet(Name = "GetWeatherForecast")]
		public async Task<IEnumerable<WeatherForecast>> Get()
		{
			return await _mediator.Send(new GetWeatherForecastQuery());
		}
	}
}
