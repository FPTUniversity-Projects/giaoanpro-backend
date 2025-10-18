using giaoanpro_backend.Application.Interfaces.Repositories;
using giaoanpro_backend.Domain.Entities;
using giaoanpro_backend.Domain.Enums;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace giaoanpro_backend.Persistence.Repositories
{
	public class AuthRepository : IAuthRepository
	{
		private readonly IGenericRepository<User> _userRepository;
		private readonly IConfiguration _configuration;
		private readonly ILogger<AuthRepository> _logger;
		private readonly string _secretKey;
		private readonly string _issuer;
		private readonly string _audience;

		public AuthRepository(IGenericRepository<User> userRepository, IConfiguration configuration, ILogger<AuthRepository> logger)
		{
			_userRepository = userRepository;
			_configuration = configuration;
			_logger = logger;
			_secretKey = _configuration["Authentication:Key"]
				?? throw new ArgumentNullException("Authentication:Key not found in configuration");
			_issuer = _configuration["Authentication:Issuer"]
				?? throw new ArgumentNullException("Authentication:Issuer not found in configuration");
			_audience = _configuration["Authentication:Audience"]
				?? throw new ArgumentNullException("Authentication:Audience not found in configuration");
		}

		public Task<string> GenerateJwtToken(User user, string role)
		{
			if (user == null) throw new ArgumentNullException(nameof(user));
			if (string.IsNullOrWhiteSpace(role)) role = "User";

			var claims = new List<Claim>
			{
				new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
				new Claim("id", user.Id.ToString()),
				new Claim("email", user.Email ?? string.Empty),
				new Claim("role", role),
				new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
			};

			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			var token = new JwtSecurityToken(
				issuer: _issuer,
				audience: _audience,
				claims: claims,
				notBefore: DateTime.UtcNow,
				expires: DateTime.UtcNow.AddHours(8),
				signingCredentials: creds
			);

			var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
			return Task.FromResult(tokenString);
		}

		// Accept optional preferred role; never assign Admin via Google registration.
		public async Task<User> RegisterViaGoogleAsync(GoogleJsonWebSignature.Payload payload, UserRole? preferredRole = null)
		{
			if (payload == null || string.IsNullOrWhiteSpace(payload.Email))
			{
				_logger.LogWarning("RegisterViaGoogleAsync called with null/invalid payload.");
				return null!;
			}

			try
			{
				var existing = await _userRepository.GetByConditionAsync(u => u.Email == payload.Email);
				if (existing != null)
				{
					_logger.LogInformation("User already exists for email {Email}. Returning existing user.", payload.Email);
					return existing;
				}

				// Decide role: prefer provided (if not Admin), otherwise default to Student.
				UserRole roleToAssign = UserRole.Student;
				if (preferredRole.HasValue && preferredRole.Value != UserRole.Admin)
				{
					roleToAssign = preferredRole.Value;
				}

				var tempPassword = GenerateTemporaryPassword(12);
				var newUser = new User
				{
					Email = payload.Email,
					Username = payload.Email,
					FullName = payload.Name ?? string.Empty,
					PasswordHash = HashPassword(tempPassword),
					IsActive = true,
					Role = roleToAssign
				};

				await _userRepository.AddAsync(newUser);
				var saved = await _userRepository.SaveChangesAsync();
				if (!saved)
				{
					_logger.LogWarning("Failed to persist new user created from Google payload for {Email}.", payload.Email);
					return null!;
				}

				_logger.LogInformation("Created new user via Google for {Email} with role {Role}.", payload.Email, roleToAssign);
				return newUser;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Unexpected error in RegisterViaGoogleAsync for email {Email}", payload?.Email);
				return null!;
			}
		}

		private static string HashPassword(string password)
		{
			if (password is null) return string.Empty;
			var bytes = Encoding.UTF8.GetBytes(password);
			var hash = SHA256.HashData(bytes);
			return Convert.ToBase64String(hash);
		}

		private string GenerateTemporaryPassword(int length = 12)
		{
			if (length < 8) length = 8;

			const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
			const string lower = "abcdefghijklmnopqrstuvwxyz";
			const string digits = "0123456789";
			const string special = "!@#$%^&*()-_=+[]{};:,.<>?";

			var all = upper + lower + digits + special;
			var chars = new List<char>
			{
				upper[RandomNumberGenerator.GetInt32(upper.Length)],
				lower[RandomNumberGenerator.GetInt32(lower.Length)],
				digits[RandomNumberGenerator.GetInt32(digits.Length)],
				special[RandomNumberGenerator.GetInt32(special.Length)]
			};

			for (int i = chars.Count; i < length; i++)
			{
				chars.Add(all[RandomNumberGenerator.GetInt32(all.Length)]);
			}

			var arr = chars.ToArray();
			for (int i = arr.Length - 1; i > 0; i--)
			{
				int j = RandomNumberGenerator.GetInt32(i + 1);
				var tmp = arr[i];
				arr[i] = arr[j];
				arr[j] = tmp;
			}

			return new string(arr);
		}
	}
}
