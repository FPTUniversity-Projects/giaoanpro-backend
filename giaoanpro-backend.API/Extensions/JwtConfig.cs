using giaoanpro_backend.Application.DTOs.Responses.Bases;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace giaoanpro_backend.API.Extensions
{
	public static class JwtConfig
	{
		public static IServiceCollection AddJWTServices(this IServiceCollection services, IConfiguration configuration)
		{
			var key = configuration["Authentication:Key"] ?? throw new ArgumentNullException("Authentication:Key not found in configuration");
			var issuer = configuration["Authentication:Issuer"] ?? throw new ArgumentNullException("Authentication:Issuer not found in configuration");
			var audience = configuration["Authentication:Audience"] ?? throw new ArgumentNullException("Authentication:Audience not found in configuration");

			services.AddAuthentication(options =>
			{
				options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
				options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
			})
			.AddJwtBearer(options =>
			{
				options.Events = new JwtBearerEvents
				{
					OnChallenge = context =>
					{
						// suppress the default WWW-Authenticate header handling and return a JSON body
						context.HandleResponse();

						context.Response.StatusCode = StatusCodes.Status401Unauthorized;
						context.Response.ContentType = "application/json";
						return context.Response.WriteAsJsonAsync(BaseResponse<string>.Fail("Missing or invalid token", ResponseErrorType.Unauthorized));
					},
					OnForbidden = context =>
					{
						context.Response.StatusCode = StatusCodes.Status403Forbidden;
						context.Response.ContentType = "application/json";
						return context.Response.WriteAsJsonAsync(BaseResponse<string>.Fail("You are not authorized to access this resource", ResponseErrorType.Forbidden));
					}
				};

				// In development you may set RequireHttpsMetadata = false. In production prefer true.
				options.RequireHttpsMetadata = false;
				options.SaveToken = true;
				options.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuer = true,
					ValidateAudience = true,
					ValidateLifetime = true,
					ValidateIssuerSigningKey = true,
					ValidIssuer = issuer,
					ValidAudience = audience,
					IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
					ClockSkew = TimeSpan.Zero
				};
			});

			return services;
		}
	}
}
