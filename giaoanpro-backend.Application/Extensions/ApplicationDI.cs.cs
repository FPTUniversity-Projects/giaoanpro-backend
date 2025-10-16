using giaoanpro_backend.Application.Interfaces.Services;
using giaoanpro_backend.Application.Services;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;

namespace giaoanpro_backend.Application.Extensions
{
	public static class ApplicationDI
	{
		public static IServiceCollection AddApplicationServices(this IServiceCollection services)
		{
			// Add application services here
			services.AddScoped<IAuthService, AuthService>();

			// Mapster configuration: clone global settings and scan this assembly for IRegister implementations
			var config = TypeAdapterConfig.GlobalSettings.Clone();

			// Register TypeAdapterConfig and Mapster IMapper (ServiceMapper)
			services.AddSingleton(config);
			services.AddScoped<IMapper, ServiceMapper>();

			// FluentValidation
			return services;
		}
	}
}
