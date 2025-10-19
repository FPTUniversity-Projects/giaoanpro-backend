using giaoanpro_backend.Application.Interfaces.Repositories;
using giaoanpro_backend.Application.Interfaces.Services._3PServices;
using giaoanpro_backend.Infrastructure._3PServices;
using giaoanpro_backend.Persistence.Context;
using giaoanpro_backend.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace giaoanpro_backend.Infrastructure.Extensions
{
	public static class InfrastructureDI
	{
		public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
		{
			// Keep explicit registrations for special types
			services.AddScoped<IUnitOfWork, UnitOfWork>();
			services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

			// Register infrastructure 3rd-party services used by Application layer
			services.AddScoped<IVnPayService, VnPayService>();

			// Convention-based registration for repository implementations in the Persistence assembly.
			// It will register classes where an interface named "I{ClassName}" exists.
			RegisterRepositoriesByConvention(services, typeof(UnitOfWork).Assembly);

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

		private static void RegisterRepositoriesByConvention(IServiceCollection services, Assembly repoAssembly)
		{
			if (repoAssembly == null) return;

			var implTypes = repoAssembly.GetTypes()
				.Where(t => t.IsClass
							&& !t.IsAbstract
							&& !t.IsGenericType
							&& t.IsPublic);

			foreach (var impl in implTypes)
			{
				// Skip types registered explicitly
				if (impl == typeof(UnitOfWork)) continue;
				if (impl.IsGenericTypeDefinition) continue;

				// find interface named I{ConcreteName}
				var match = impl.GetInterfaces()
					.FirstOrDefault(i => i.Name == $"I{impl.Name}");

				if (match == null) continue;

				// Avoid registering open-generic mapping or duplicates (IGenericRepository<> handled explicitly)
				if (match.IsGenericType) continue;

				services.AddScoped(match, impl);
			}
		}
	}
}
