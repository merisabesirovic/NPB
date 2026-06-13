using Microsoft.EntityFrameworkCore;
using RideMateAPI.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using RideMateAPI.Models;
using System.Security.Claims;
using DotNetEnv;
using System.IO;
using Microsoft.AspNetCore.Identity;
using RideMateAPI.Middleware;
using Asp.Versioning;
using FluentValidation;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using RideMateAPI.Application.Common;
using RideMateAPI.Infrastructure;
using Serilog;

Log.Logger = new LoggerConfiguration()
	.Enrich.FromLogContext()
	.WriteTo.Console()
	.WriteTo.File("Logs/ridemate-.log", rollingInterval: RollingInterval.Day)
	.CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((context, services, configuration) =>
{
	configuration
		.ReadFrom.Configuration(context.Configuration)
		.ReadFrom.Services(services)
		.Enrich.FromLogContext()
		.WriteTo.Console()
		.WriteTo.File("Logs/ridemate-.log", rollingInterval: RollingInterval.Day);
});

// Add services to the container.

// Load .env file into environment: try project content root, then AppContext.BaseDirectory, then current directory
var envCandidates = new[]
{
	Path.Combine(builder.Environment.ContentRootPath, ".env"),
	Path.Combine(AppContext.BaseDirectory ?? string.Empty, ".env"),
	Path.Combine(Directory.GetCurrentDirectory(), ".env")
};
string? loadedFrom = null;
foreach (var candidate in envCandidates)
{
	if (!string.IsNullOrWhiteSpace(candidate) && File.Exists(candidate))
	{
		DotNetEnv.Env.Load(candidate);
		loadedFrom = candidate;
		break;
	}
}
if (loadedFrom != null)
{
	Log.Information("Loaded .env from {EnvPath}", loadedFrom);
}
else
{
	Log.Information("No .env file found in ContentRoot, AppContext.BaseDirectory or CurrentDirectory.");
}

builder.Services.AddControllers();
var frontendOrigins = new[] { "http://localhost:5173", "https://localhost:5173" }
	.Concat((builder.Configuration["FrontendUrl"] ?? string.Empty)
		.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
	.Distinct(StringComparer.OrdinalIgnoreCase)
	.ToArray();

builder.Services.AddCors(options =>
{
	options.AddPolicy("Frontend", policy =>
	{
		policy
			.WithOrigins(frontendOrigins)
			.AllowAnyHeader()
			.AllowAnyMethod();
	});
});
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
	options.InvalidModelStateResponseFactory = context =>
	{
		var problemDetails = new ValidationProblemDetails(context.ModelState)
		{
			Status = StatusCodes.Status400BadRequest,
			Title = "One or more validation errors occurred.",
			Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
			Instance = context.HttpContext.Request.Path
		};
		problemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
		return new BadRequestObjectResult(problemDetails);
	};
});
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ProblemDetailsExceptionHandler>();
builder.Services.AddAutoMapper(cfg => cfg.AddMaps(typeof(Program).Assembly));
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services
	.AddApiVersioning(options =>
	{
		options.DefaultApiVersion = new ApiVersion(1, 0);
		options.AssumeDefaultVersionWhenUnspecified = true;
		options.ReportApiVersions = true;
		options.ApiVersionReader = ApiVersionReader.Combine(
			new QueryStringApiVersionReader("api-version"),
			new HeaderApiVersionReader("X-Api-Version"));
	})
	.AddMvc()
	.AddApiExplorer(options =>
	{
		options.GroupNameFormat = "'v'VVV";
		options.SubstituteApiVersionInUrl = true;
	});
// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
	c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
	{
		Title = "RideMate API",
		Version = "v1"
	});

	// Add JWT bearer definition so Swagger UI shows Authorize button
	c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
	{
		Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
		Name = "Authorization",
		In = Microsoft.OpenApi.Models.ParameterLocation.Header,
		Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
		Scheme = "bearer",
		BearerFormat = "JWT"
	});

	c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
	{
		{
			new Microsoft.OpenApi.Models.OpenApiSecurityScheme
			{
				Reference = new Microsoft.OpenApi.Models.OpenApiReference
				{
					Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
					Id = "Bearer"
				}
			},
			new string[] { }
		}
	});
});
// Register DbContext before building the app so design-time tools can resolve it
var dbConnectionString = PostgresConnectionString.FromConfiguration(builder.Configuration)
	?? throw new InvalidOperationException("No database connection string configured. Set RIDE_MATE_CONNECTION, DATABASE_URL or ConnectionStrings:DefaultConnection.");
builder.Services.AddDbContext<RideMateDbContext>(options => options.UseNpgsql(dbConnectionString));

// Password hasher for user passwords
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// Cloudinary service
builder.Services.AddSingleton<RideMateAPI.Services.CloudinaryService>();
// Ride service
builder.Services.AddScoped<RideMateAPI.Services.RideService>();
// Booking service
builder.Services.AddScoped<RideMateAPI.Services.BookingService>();
builder.Services.AddScoped<RideMateAPI.Services.ReviewService>();
builder.Services.AddScoped<RideMateAPI.Services.NotificationService>();
builder.Services.AddScoped<RideMateAPI.Services.DisputeService>();
builder.Services.AddScoped<RideMateAPI.Services.UserService>();
builder.Services.AddScoped<RideMateAPI.Services.TokenService>();
builder.Services.AddScoped<RideMateAPI.Services.AuthSessionService>();

// JWT configuration (reads from configuration or uses defaults for dev)
var jwtKey = builder.Configuration["Jwt:Key"] ?? "super_secret_development_key_change_this";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "RideMateApi";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "RideMateApiClients";

var keyBytes = Encoding.UTF8.GetBytes(jwtKey);
builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
	.AddJwtBearer(options =>
	{
		options.RequireHttpsMetadata = false;
		options.SaveToken = true;
		options.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuer = true,
			ValidIssuer = jwtIssuer,
			ValidateAudience = true,
			ValidAudience = jwtAudience,
			ValidateIssuerSigningKey = true,
			IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            ValidateLifetime = true,
			ClockSkew = TimeSpan.FromMinutes(2),
			// Ensure role claim mapping uses ClaimTypes.Role so [Authorize(Roles="...")] works
			RoleClaimType = ClaimTypes.Role,
			NameClaimType = ClaimTypes.Name
		};

		options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
		{
			OnTokenValidated = ctx =>
			{
				var logger = ctx.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("JwtBearer");
				logger.LogDebug("JWT validated for {User}", ctx.Principal?.Identity?.Name);
				return Task.CompletedTask;
			},
			OnAuthenticationFailed = ctx =>
			{
				var logger = ctx.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("JwtBearer");
				logger.LogWarning(ctx.Exception, "JWT authentication failed on {Path}", ctx.Request.Path);
				return Task.CompletedTask;
			}
		};
	});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Serve generated OpenAPI/Swagger JSON and the Swagger UI
	app.UseSwagger();
	app.UseSwaggerUI();
}
app.UseExceptionHandler();
app.UseSerilogRequestLogging();
if (!app.Environment.IsProduction())
{
	app.UseHttpsRedirection();
}

app.UseCors("Frontend");

app.UseAuthentication();

// Slow request middleware should run after authentication so User claims are populated
app.UseSlowRequestMiddleware();

app.UseAuthorization();

app.MapControllers();

app.Run();
