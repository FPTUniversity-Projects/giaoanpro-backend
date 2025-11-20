using Amazon.S3;
using Amazon.Runtime;
using giaoanpro_backend.Application.Interfaces.Repositories.Bases;
using giaoanpro_backend.Application.Interfaces.Services._3PServices;
using giaoanpro_backend.Infrastructure._3PServices;
using giaoanpro_backend.Persistence.Context;
using giaoanpro_backend.Persistence.Repositories.Bases;
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
			services.AddHttpClient();
			services.AddScoped<IGeminiService, GeminiService>();

			// Register infrastructure 3rd-party services used by Application layer
			services.AddScoped<IVnPayService, VnPayService>();
			services.AddScoped<IS3Service, S3Service>();

			// Register AWS S3 client with environment variables
			// AWS SDK will automatically use environment variables:
			// - AWS_ACCESS_KEY_ID
			// - AWS_SECRET_ACCESS_KEY
			// - AWS_REGION or AWS_DEFAULT_REGION
			services.AddSingleton<IAmazonS3>(sp =>
			{
				var region = Environment.GetEnvironmentVariable("AWS_REGION")
					?? Environment.GetEnvironmentVariable("AWS_DEFAULT_REGION")
					?? "us-east-1";

				var config = new Amazon.S3.AmazonS3Config
				{
					RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(region),
					Timeout = TimeSpan.FromSeconds(60),
					MaxErrorRetry = 3,
					// Force path style for better compatibility
					ForcePathStyle = false,
					// Use HTTPS
					UseHttp = false
				};

				// Check if credentials are provided via environment variables
				var accessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID");
				var secretKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY");

				if (!string.IsNullOrWhiteSpace(accessKey) && !string.IsNullOrWhiteSpace(secretKey))
				{
					// Use explicit credentials from environment variables
					var credentials = new BasicAWSCredentials(accessKey, secretKey);
					return new Amazon.S3.AmazonS3Client(credentials, config);
				}
				else
				{
					// Fall back to default credential chain (IAM roles, credentials file, etc.)
					return new Amazon.S3.AmazonS3Client(config);
				}
			});

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
				.Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericType && t.IsPublic);

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
