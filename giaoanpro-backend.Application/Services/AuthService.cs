using giaoanpro_backend.Application.DTOs.Requests.Auths;
using giaoanpro_backend.Application.DTOs.Responses.Auths;
using giaoanpro_backend.Application.DTOs.Responses.Bases;
using giaoanpro_backend.Application.Interfaces.Repositories;
using giaoanpro_backend.Application.Interfaces.Services;
using giaoanpro_backend.Domain.Entities;
using giaoanpro_backend.Domain.Enums;
using Google.Apis.Auth;
using System.Security.Cryptography;
using System.Text;

namespace giaoanpro_backend.Application.Services
{
	public class AuthService : IAuthService
	{
		private readonly IUserRepository _userRepository;
		private readonly IAuthRepository _authRepository;

		public AuthService(IUserRepository userRepository, IAuthRepository authRepository)
		{
			_userRepository = userRepository;
			_authRepository = authRepository;
		}

		public async Task<BaseResponse<TokenResponse>> LoginAsync(LoginRequest request)
		{
			if (request is null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
				return BaseResponse<TokenResponse>.Fail("Invalid login request", ResponseErrorType.BadRequest);

			var user = await _userRepository.GetByEmailAsync(request.Email);
			// For security do not reveal whether a user exists. Treat missing user as invalid credentials (Unauthorized).
			if (user == null)
				return BaseResponse<TokenResponse>.Fail("Invalid email or password.", ResponseErrorType.Unauthorized);

			if (!user.IsActive)
				return BaseResponse<TokenResponse>.Fail("User is inactive", ResponseErrorType.Forbidden);

			// Validate password by comparing stored hash
			var providedHash = HashPassword(request.Password);
			if (!string.Equals(providedHash, user.PasswordHash, StringComparison.Ordinal))
				return BaseResponse<TokenResponse>.Fail("Invalid email or password.", ResponseErrorType.Unauthorized);

			var tokenResponse = GenerateTokenResponseAsync(user);
			return BaseResponse<TokenResponse>.Ok(tokenResponse);
		}

		private TokenResponse GenerateTokenResponseAsync(User user)
		{
			var role = user.Role.ToString();
			var token = _authRepository.GenerateJwtToken(user, role);
			return new TokenResponse()
			{
				AccessToken = token,
				Role = user.Role
			};
		}

		public async Task<BaseResponse<TokenResponse>> LoginWithGoogleAsync(GoogleLoginRequest request, UserRole? preferredRole = null)
		{
			if (request is null || string.IsNullOrWhiteSpace(request.IdToken))
				return BaseResponse<TokenResponse>.Fail("Invalid request", ResponseErrorType.BadRequest);

			GoogleJsonWebSignature.Payload? payload;
			try
			{
				payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken);
			}
			catch
			{
				return BaseResponse<TokenResponse>.Fail("Invalid Google token", ResponseErrorType.BadRequest);
			}

			if (payload is null)
				return BaseResponse<TokenResponse>.Fail("Invalid Google token payload", ResponseErrorType.BadRequest);

			var email = payload.Email;
			if (string.IsNullOrWhiteSpace(email))
				return BaseResponse<TokenResponse>.Fail("Google token does not contain an email", ResponseErrorType.BadRequest);

			User? user = null;
			try
			{
				user = await _userRepository.GetByEmailAsync(email);
				if (user is null)
				{
					// Create the user and apply preferredRole if provided (but never Admin).
					user = await _authRepository.RegisterViaGoogleAsync(payload, preferredRole);
					if (user is null)
						return BaseResponse<TokenResponse>.Fail("Failed to create user account from Google payload", ResponseErrorType.InternalError);
				}
			}
			catch (Exception ex)
			{
				return BaseResponse<TokenResponse>.Fail("Google registration failed: " + ex.Message, ResponseErrorType.InternalError);
			}

			if (!user.IsActive)
				return BaseResponse<TokenResponse>.Fail("Your account is inactive.", ResponseErrorType.Forbidden);

			if (user.Role == UserRole.Admin)
				return BaseResponse<TokenResponse>.Fail("Admin accounts must be created by an administrator.", ResponseErrorType.Forbidden);

			var tokenResponse = GenerateTokenResponseAsync(user);

			var message = "Google login successful";
			return BaseResponse<TokenResponse>.Ok(tokenResponse, message);
		}

		public async Task<BaseResponse<string>> RegisterAsync(RegisterRequest request, UserRole role)
		{
			if (request is null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
				return BaseResponse<string>.Fail("Invalid registration request", ResponseErrorType.BadRequest);

			var existingUser = await _userRepository.GetByEmailOrUsernameAsync(request.Email, request.Username);
			if (existingUser != null)
				return BaseResponse<string>.Fail("Email or username already exists", ResponseErrorType.Conflict);

			var user = new User
			{
				FullName = request.FullName ?? string.Empty,
				Email = request.Email,
				Username = request.Username,
				IsActive = true,
				Role = role,
				PasswordHash = HashPassword(request.Password)
			};

			await _userRepository.AddAsync(user);
			var saved = await _userRepository.SaveChangesAsync();
			if (!saved)
				return BaseResponse<string>.Fail("Failed to create user", ResponseErrorType.InternalError);

			return BaseResponse<string>.Ok("User registered successfully");
		}

		// Simple SHA256-based hash for storing passwords without Identity.
		// For production consider using a stronger KDF (e.g., PBKDF2, Argon2, bcrypt) with a per-user salt.
		private static string HashPassword(string password)
		{
			if (password is null) return string.Empty;
			var bytes = Encoding.UTF8.GetBytes(password);
			var hash = SHA256.HashData(bytes);
			return Convert.ToBase64String(hash);
		}
	}
}
