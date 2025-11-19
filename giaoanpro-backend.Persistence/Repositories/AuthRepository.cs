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
		private readonly IUserRepository _userRepository;
		private readonly IConfiguration _configuration;
		private readonly ILogger<AuthRepository> _logger;
		private readonly string _secretKey;
		private readonly string _issuer;
		private readonly string _audience;
		private readonly int _expiryMinutes;

		public AuthRepository(IUserRepository userRepository, IConfiguration configuration, ILogger<AuthRepository> logger)
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
			_expiryMinutes = int.TryParse(configuration["Authentication:ExpiryInMinutes"], out var minutes)
				? minutes
				: throw new ArgumentNullException("Authentication:ExpiryInMinutes not found or invalid");
		}

		public string GenerateJwtToken(User user, string role)
		{
			if (user == null) throw new ArgumentNullException(nameof(user));
			if (string.IsNullOrWhiteSpace(role)) role = UserRole.Student.ToString();

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
				expires: DateTime.UtcNow.AddMinutes(_expiryMinutes),
				signingCredentials: creds
			);

			var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
			return tokenString;
		}

		public ClaimsPrincipal? GetPrincipalFromExpiredToken(string? token)
		{
			var tokenValidationParameters = new TokenValidationParameters
			{
				ValidateAudience = false, // Có thể để true nếu cần
				ValidateIssuer = false,   // Có thể để true nếu cần
				ValidateIssuerSigningKey = true,
				IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey)), // Dùng _secretKey của bạn
				ValidateLifetime = false // QUAN TRỌNG: Bỏ qua lỗi hết hạn
			};

			var tokenHandler = new JwtSecurityTokenHandler();
			try
			{
				var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

				// Kiểm tra thuật toán mã hóa để tránh tấn công giả mạo
				if (!(securityToken is JwtSecurityToken jwtSecurityToken) ||
					!jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
				{
					throw new SecurityTokenException("Invalid token");
				}

				return principal;
			}
			catch
			{
				return null;
			}
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
				var existing = await _userRepository.GetByEmailAsync(payload.Email);
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

		private static string GenerateTemporaryPassword(int length = 12)
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
				(arr[j], arr[i]) = (arr[i], arr[j]);
			}

			return new string(arr);
		}

		public async Task<string> GenerateAndSaveRefreshToken(User user)
		{
			if (user == null) throw new ArgumentNullException(nameof(user));

			var refreshToken = GenerateRefreshToken();
			user.RefreshToken = refreshToken;
			user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

			// Ensure repository tracks update
			_userRepository.Update(user);
			var saved = await _userRepository.SaveChangesAsync();
			if (!saved)
			{
				_logger.LogWarning("Failed to save refresh token for user {UserId}", user.Id);
				// still return the token but caller should be aware persistence failed
			}

			return refreshToken;
		}

		public async Task<User?> ValidateRefreshToken(Guid userId, string refreshToken)
		{
			if (userId == Guid.Empty || string.IsNullOrWhiteSpace(refreshToken)) return null;
			var user = await _userRepository.GetByIdAsync(userId);
			if (user == null) return null;
			if (string.IsNullOrWhiteSpace(user.RefreshToken)) return null;
			if (!string.Equals(user.RefreshToken, refreshToken, StringComparison.Ordinal)) return null;
			if (!user.RefreshTokenExpiryTime.HasValue || user.RefreshTokenExpiryTime.Value < DateTime.UtcNow) return null;
			return user;
		}

		private string GenerateRefreshToken()
		{
			var randomNumber = new byte[32];
			using (var rng = RandomNumberGenerator.Create())
			{
				rng.GetBytes(randomNumber);
			}
			return Convert.ToBase64String(randomNumber);
		}

		// Revoke a user's refresh token by clearing the stored token and expiry, then persisting the change.
		public async Task<bool> RevokeRefreshToken(Guid userId)
		{
			if (userId == Guid.Empty) return false;

			var user = await _userRepository.GetByIdAsync(userId);
			if (user == null)
			{
				_logger.LogWarning("Attempted to revoke refresh token for non-existent user {UserId}", userId);
				return false;
			}

			user.RefreshToken = string.Empty;
			user.RefreshTokenExpiryTime = null;
			_userRepository.Update(user);
			var saved = await _userRepository.SaveChangesAsync();
			if (!saved)
			{
				_logger.LogWarning("Failed to persist revoked refresh token for user {UserId}", userId);
				return false;
			}

			_logger.LogInformation("Revoked refresh token for user {UserId}", userId);
			return true;
		}
	}
}
