using giaoanpro_backend.API.Extensions;
using giaoanpro_backend.Application.Extensions;
using giaoanpro_backend.Infrastructure.Extensions;
using giaoanpro_backend.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Load .env manually in Development
if (builder.Environment.IsDevelopment())
{
	string solutionRootPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
	string envFile = Path.Combine(solutionRootPath, ".env");

	if (File.Exists(envFile))
	{
		var lines = File.ReadAllLines(envFile);

		foreach (var line in lines)
		{
			if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#"))
				continue;

			var split = line.Split('=', 2);
			if (split.Length != 2)
				continue;

			var key = split[0].Trim();
			var value = split[1].Trim();

			if (value.StartsWith("\"") && value.EndsWith("\""))
				value = value[1..^1];

			Environment.SetEnvironmentVariable(key, value);
		}
	}
}

// Add config sources
builder.Configuration
	.SetBasePath(Directory.GetCurrentDirectory())
	.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
	.AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddJWTServices(builder.Configuration);

// Force all routes to be lowercase
builder.Services.AddRouting(options => options.LowercaseUrls = true);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
	c.SwaggerDoc("v1", new OpenApiInfo
	{
		Title = "StudeeHub_API",
		Version = "v1"
	});
	c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
	{
		Name = "Authorization",
		//Type = SecuritySchemeType.ApiKey,
		Type = SecuritySchemeType.Http,
		Scheme = "Bearer",
		BearerFormat = "JWT",
		In = ParameterLocation.Header,
		Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter Your token in the text input below."
	});
	c.AddSecurityRequirement(new OpenApiSecurityRequirement()
	{
		{
			new OpenApiSecurityScheme
			{
				Reference = new OpenApiReference
				{
					Type = ReferenceType.SecurityScheme,
					Id = "Bearer"
				},
			},
			new List<string>()
		}
	});
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowFrontend");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
ApplyMigration();
app.Run();


void ApplyMigration()
{
	using (var scope = app.Services.CreateScope())
	{
		var _db = scope.ServiceProvider.GetRequiredService<GiaoanproDBContext>();

		if (_db.Database.GetPendingMigrations().Count() > 0)
		{
			_db.Database.Migrate();
		}
	}
}