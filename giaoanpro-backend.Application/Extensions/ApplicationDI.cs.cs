using FluentValidation;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace giaoanpro_backend.Application.Extensions
{
	public static class ApplicationDI
	{
		public static IServiceCollection AddApplicationServices(this IServiceCollection services)
		{
			// Use the current assembly for scanning
			var assembly = typeof(ApplicationDI).Assembly;

			// Register implementations that follow the convention: I{TypeName} -> {TypeName}
			RegisterServicesByConvention(services, assembly);

			// Mapster configuration: clone global settings and scan this assembly for IRegister implementations
			var config = TypeAdapterConfig.GlobalSettings.Clone();
			config.Scan(assembly);
			services.AddSingleton(config);
			services.AddScoped<IMapper, ServiceMapper>();

			// FluentValidation - registers all validators found in the assembly
			services.AddValidatorsFromAssembly(assembly);

			return services;
		}

		// Registers concrete types against the interface named "I{ConcreteTypeName}" as Scoped.
		// This reduces manual AddScoped(...) calls for each service class.
		private static void RegisterServicesByConvention(IServiceCollection services, Assembly assembly)
		{
			var types = assembly.GetTypes()
				.Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericType && t.IsPublic);

			foreach (var impl in types)
			{
				// Find an interface that matches the convention I{TypeName}
				var match = impl.GetInterfaces()
					.FirstOrDefault(i => i.Name == $"I{impl.Name}");

				if (match != null)
				{
					services.AddScoped(match, impl);
				}
			}
		}
	}
}
