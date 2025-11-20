using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace giaoanpro_backend.API.Extensions
{
	/// <summary>
	/// Custom operation filter to handle IFormFile parameters in Swagger UI
	/// Converts IFormFile parameters to multipart/form-data schema
	/// </summary>
	public class FileUploadOperationFilter : IOperationFilter
	{
		public void Apply(OpenApiOperation operation, OperationFilterContext context)
		{
			var formFileParams = context.ApiDescription.ParameterDescriptions
				.Where(p => p.ModelMetadata?.ModelType == typeof(IFormFile) || 
				           p.ModelMetadata != null && IsFormFileType(p.ModelMetadata.ModelType))
				.ToList();

			if (!formFileParams.Any()) return;

			// Set request body to multipart/form-data
			operation.RequestBody = new OpenApiRequestBody
			{
				Content = new Dictionary<string, OpenApiMediaType>
				{
					["multipart/form-data"] = new OpenApiMediaType
					{
						Schema = new OpenApiSchema
						{
							Type = "object",
							Properties = context.ApiDescription.ParameterDescriptions
								.ToDictionary(
									p => p.Name,
									p => IsFormFileType(p.ModelMetadata?.ModelType)
										? new OpenApiSchema { Type = "string", Format = "binary" }
										: context.SchemaGenerator.GenerateSchema(
											p.ModelMetadata?.ModelType ?? typeof(string), 
											context.SchemaRepository)
								),
							Required = context.ApiDescription.ParameterDescriptions
								.Where(p => !p.IsOptional())
								.Select(p => p.Name)
								.ToHashSet()
						}
					}
				}
			};

			// Remove IFormFile parameters from operation.Parameters (they're now in requestBody)
			foreach (var param in formFileParams)
			{
				var paramToRemove = operation.Parameters.FirstOrDefault(p => p.Name == param.Name);
				if (paramToRemove != null)
					operation.Parameters.Remove(paramToRemove);
			}
		}

		private static bool IsFormFileType(Type? type)
		{
			if (type == null) return false;
			return type == typeof(IFormFile) || 
			       Nullable.GetUnderlyingType(type) == typeof(IFormFile);
		}
	}

	/// <summary>
	/// Extension methods for ApiParameterDescription
	/// </summary>
	public static class ParameterDescriptionExtensions
	{
		/// <summary>
		/// Checks if a parameter is optional (nullable, has default value, or marked as optional)
		/// </summary>
		public static bool IsOptional(this ApiParameterDescription parameter)
		{
			return parameter.DefaultValue != null ||
				   parameter.RouteInfo?.IsOptional == true ||
				   parameter.Type == null ||
				   parameter.Type.IsGenericType && 
				    parameter.Type.GetGenericTypeDefinition() == typeof(Nullable<>);
		}
	}
}
