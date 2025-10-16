using giaoanpro_backend.Application.Interfaces.Repositories;
using giaoanpro_backend.Persistence.Context;
using giaoanpro_backend.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace giaoanpro_backend.Infrastructure.Extensions
{
	public static class InfrastructureDI
	{
		public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
		{
			// Register Repositories
			services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
			services.AddScoped<IAuthRepository, AuthRepository>();

			// Register Third-Party Services (e.g., Email, SMS)

			// DBContext
			var connectionString = configuration["DATABASE_CONNECTION_STRING"];

			if (string.IsNullOrWhiteSpace(connectionString))
			{
				throw new InvalidOperationException("DATABASE_CONNECTION_STRING is not configured.");
			}

			services.AddDbContext<GiaoanproDBContext>(options =>
				options.UseSqlServer(connectionString));

			// CORS
			var webUrl = configuration["Front-end:webUrl"] ?? throw new Exception("Missing web url!!");
			services.AddCors(options =>
			{
				options.AddPolicy("AllowFrontend", builder =>
				{
					builder
						.WithOrigins(webUrl)
						.AllowAnyHeader()
						.AllowAnyMethod()
						.AllowCredentials();
				});
			});

			return services;
		}
	}
}
