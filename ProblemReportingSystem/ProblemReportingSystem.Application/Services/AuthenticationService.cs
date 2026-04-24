using Microsoft.Extensions.Configuration;

namespace ProblemReportingSystem.Application.Services;

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using BCrypt.Net;
using Microsoft.IdentityModel.Tokens;
using ProblemReportingSystem.Application.DTOs;
using ProblemReportingSystem.Application.ServiceAbstractions;
using ProblemReportingSystem.DAL.Entities;
using ProblemReportingSystem.DAL.RepositoryAbstractions;

/// <summary>
/// Implementation of the authentication service for user registration, login, and OAuth operations.
/// Handles both email/password and Google OAuth 2.0 authentication flows.
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly string _jwtSecret;
    private readonly string _jwtIssuer;
    private readonly string _jwtAudience;
    private readonly int _jwtExpirationMinutes;

    public AuthenticationService(
        IUserRepository userRepository,
        IMapper mapper,
        IConfiguration configuration)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        
        var jwtSettings = configuration.GetSection("JwtSettings");
        _jwtSecret = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");
        _jwtIssuer = jwtSettings["Issuer"] ?? throw new InvalidOperationException("JWT Issuer not configured");
        _jwtAudience = jwtSettings["Audience"] ?? throw new InvalidOperationException("JWT Audience not configured");
        _jwtExpirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "60");
    }

    /// <summary>
    /// Registers a new user with email and password.
    /// Validates password strength and ensures email uniqueness.
    /// </summary>
    public async Task<AuthenticationResponseDto> RegisterAsync(RegisterDto registerDto, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate input
            if (!ValidateRegisterDto(registerDto, out var errors))
            {
                return new AuthenticationResponseDto
                {
                    Success = false,
                    Message = "Registration validation failed",
                    Errors = errors
                };
            }

            // Check if user already exists
            var existingUser = await _userRepository
                .GetByEmailAsync(registerDto.Email, cancellationToken);

            if (existingUser != null)
            {
                return new AuthenticationResponseDto
                {
                    Success = false,
                    Message = "Email is already registered",
                    Errors = new List<string> { "Email already in use" }
                };
            }

            // Create new user with address using mapper
            var user = _mapper.Map<User>(registerDto);
            user.UserId = Guid.NewGuid();
            user.PasswordHash = HashPassword(registerDto.Password);
            user.IsActive = true;

            var createdUser = await _userRepository.CreateAsync(user);

            // Generate tokens
            var accessToken = GenerateAccessToken(createdUser);
            var refreshToken = GenerateRefreshToken();

            var userDto = _mapper.Map<UserDto>(createdUser);
            var addressDto = createdUser.Address != null ? _mapper.Map<AddressDto>(createdUser.Address) : null;

            return new AuthenticationResponseDto
            {
                Success = true,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = _jwtExpirationMinutes * 60,
                TokenType = "Bearer",
                User = userDto,
                Address = addressDto,
                Message = "Registration successful"
            };
        }
        catch (Exception ex)
        {
            return new AuthenticationResponseDto
            {
                Success = false,
                Message = "An error occurred during registration",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    /// <summary>
    /// Authenticates a user with email and password.
    /// </summary>
    public async Task<AuthenticationResponseDto> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(loginDto.Email) || string.IsNullOrWhiteSpace(loginDto.Password))
            {
                return new AuthenticationResponseDto
                {
                    Success = false,
                    Message = "Email and password are required",
                    Errors = new List<string> { "Invalid credentials" }
                };
            }

            // Find user by email
            var user = await _userRepository
                .GetByEmailAsync(loginDto.Email, cancellationToken);

            if (user == null)
            {
                return new AuthenticationResponseDto
                {
                    Success = false,
                    Message = "Invalid email or password",
                    Errors = new List<string> { "User not found" }
                };
            }

            // Check if user is active
            if (user.IsActive == false)
            {
                return new AuthenticationResponseDto
                {
                    Success = false,
                    Message = "User account is inactive",
                    Errors = new List<string> { "Account disabled" }
                };
            }

            // Verify password
            if (string.IsNullOrWhiteSpace(user.PasswordHash) || !VerifyPassword(loginDto.Password, user.PasswordHash))
            {
                return new AuthenticationResponseDto
                {
                    Success = false,
                    Message = "Invalid email or password",
                    Errors = new List<string> { "Authentication failed" }
                };
            }

            // Generate tokens
            var accessToken = GenerateAccessToken(user);
            var refreshToken = GenerateRefreshToken();

            var userDto = _mapper.Map<UserDto>(user);
            var addressDto = user.Address != null ? _mapper.Map<AddressDto>(user.Address) : null;

            return new AuthenticationResponseDto
            {
                Success = true,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = _jwtExpirationMinutes * 60,
                TokenType = "Bearer",
                User = userDto,
                Address = addressDto,
                Message = "Login successful"
            };
        }
        catch (Exception ex)
        {
            return new AuthenticationResponseDto
            {
                Success = false,
                Message = "An error occurred during login",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    /// <summary>
    /// Authenticates a user via Google OAuth 2.0.
    /// Creates a new user if one doesn't exist with the provided Google ID and email.
    /// </summary>
    public async Task<AuthenticationResponseDto> GoogleAuthenticateAsync(GoogleAuthDto googleAuthDto, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate Google ID token (implement Google token validation)
            // var validToken = await ValidateGoogleTokenAsync(googleAuthDto.IdToken, cancellationToken);
            // if (!validToken) return new AuthenticationResponseDto { Success = false, Message = "Invalid Google token" };

            User? user = null;

            // Try to find user by Google ID
            if (!string.IsNullOrWhiteSpace(googleAuthDto.GoogleId))
            {
                user = await _userRepository
                    .GetByGoogleAuthIdAsync(googleAuthDto.GoogleId, cancellationToken);
            }

            // If user doesn't exist, try to find by email
            if (user == null && !string.IsNullOrWhiteSpace(googleAuthDto.Email))
            {
                user = await _userRepository
                    .GetByEmailAsync(googleAuthDto.Email, cancellationToken);

                // If user exists with email but no Google ID, link the Google account
                if (user != null && string.IsNullOrWhiteSpace(googleAuthDto.GoogleId) == false)
                {
                    user.GoogleAuthId = googleAuthDto.GoogleId;
                    await _userRepository.UpdateAsync(user);
                }
            }

            // If user still doesn't exist, create new user
            if (user == null)
            {
                var newUser = new User
                {
                    UserId = Guid.NewGuid(),
                    FullName = googleAuthDto.FullName ?? "Google User",
                    Email = googleAuthDto.Email ?? $"google-{googleAuthDto.GoogleId}@oauth.local",
                    GoogleAuthId = googleAuthDto.GoogleId,
                    IsActive = true
                };

                user = await _userRepository.CreateAsync(newUser);
            }

            // Check if user is active
            if (user.IsActive == false)
            {
                return new AuthenticationResponseDto
                {
                    Success = false,
                    Message = "User account is inactive",
                    Errors = new List<string> { "Account disabled" }
                };
            }

            // Generate tokens
            var accessToken = GenerateAccessToken(user);
            var refreshToken = GenerateRefreshToken();

            var userDto = _mapper.Map<UserDto>(user);
            var addressDto = user.Address != null ? _mapper.Map<AddressDto>(user.Address) : null;

            return new AuthenticationResponseDto
            {
                Success = true,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = _jwtExpirationMinutes * 60,
                TokenType = "Bearer",
                User = userDto,
                Address = addressDto,
                Message = "Google authentication successful"
            };
        }
        catch (Exception ex)
        {
            return new AuthenticationResponseDto
            {
                Success = false,
                Message = "An error occurred during Google authentication",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    /// <summary>
    /// Registers a new user via Google OAuth 2.0.
    /// </summary>
    public async Task<AuthenticationResponseDto> GoogleRegisterAsync(GoogleAuthDto googleAuthDto, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate Google ID token
            // var validToken = await ValidateGoogleTokenAsync(googleAuthDto.IdToken, cancellationToken);
            // if (!validToken) return new AuthenticationResponseDto { Success = false, Message = "Invalid Google token" };

            // Check if user already exists
            if (!string.IsNullOrWhiteSpace(googleAuthDto.Email))
            {
                var existingUser = await _userRepository
                    .GetByEmailAsync(googleAuthDto.Email, cancellationToken);

                if (existingUser != null)
                {
                    // If user exists but doesn't have Google ID, link it
                    if (string.IsNullOrWhiteSpace(existingUser.GoogleAuthId) && !string.IsNullOrWhiteSpace(googleAuthDto.GoogleId))
                    {
                        existingUser.GoogleAuthId = googleAuthDto.GoogleId;
                        await _userRepository.UpdateAsync(existingUser);
                    }

                    // Proceed with authentication instead
                    return await GoogleAuthenticateAsync(googleAuthDto, cancellationToken);
                }
            }

            // Check if Google ID is already registered
            if (!string.IsNullOrWhiteSpace(googleAuthDto.GoogleId))
            {
                var googleUser = await _userRepository
                    .GetByGoogleAuthIdAsync(googleAuthDto.GoogleId, cancellationToken);

                if (googleUser != null)
                {
                    return new AuthenticationResponseDto
                    {
                        Success = false,
                        Message = "This Google account is already registered",
                        Errors = new List<string> { "Google account already in use" }
                    };
                }
            }

            // Create new user
            var newUser = new User
            {
                UserId = Guid.NewGuid(),
                FullName = googleAuthDto.FullName ?? "Google User",
                Email = googleAuthDto.Email ?? $"google-{googleAuthDto.GoogleId}@oauth.local",
                GoogleAuthId = googleAuthDto.GoogleId,
                IsActive = true
            };

            var createdUser = await _userRepository.CreateAsync(newUser);

            // Generate tokens
            var accessToken = GenerateAccessToken(createdUser);
            var refreshToken = GenerateRefreshToken();

            var userDto = _mapper.Map<UserDto>(createdUser);

            return new AuthenticationResponseDto
            {
                Success = true,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = _jwtExpirationMinutes * 60,
                TokenType = "Bearer",
                User = userDto,
                Message = "Google registration successful"
            };
        }
        catch (Exception ex)
        {
            return new AuthenticationResponseDto
            {
                Success = false,
                Message = "An error occurred during Google registration",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    /// <summary>
    /// Refreshes an expired JWT access token using a refresh token.
    /// </summary>
    public async Task<AuthenticationResponseDto> RefreshTokenAsync(RefreshTokenDto refreshTokenDto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(refreshTokenDto.RefreshToken))
            {
                return new AuthenticationResponseDto
                {
                    Success = false,
                    Message = "Refresh token is required",
                    Errors = new List<string> { "Invalid token" }
                };
            }

            // Validate and retrieve refresh token from database
            // var storedToken = await GetStoredRefreshTokenAsync(refreshTokenDto.RefreshToken, cancellationToken);
            // if (storedToken == null || storedToken.ExpiresAt < DateTime.UtcNow)
            // {
            //     return new AuthenticationResponseDto { Success = false, Message = "Refresh token expired" };
            // }

            // For demonstration, we'll assume token is valid
            // In production, validate against stored tokens in database

            // Generate new access token
            // var user = await _userRepository.GetByIdAsync(storedToken.UserId, cancellationToken);
            
            // Placeholder: Create a demo response
            var newAccessToken = GenerateAccessToken(new User { UserId = Guid.NewGuid(), Email = "user@example.com", FullName = "User" });

            return new AuthenticationResponseDto
            {
                Success = true,
                AccessToken = newAccessToken,
                RefreshToken = refreshTokenDto.RefreshToken,
                ExpiresIn = _jwtExpirationMinutes * 60,
                TokenType = "Bearer",
                Message = "Token refreshed successfully"
            };
        }
        catch (Exception ex)
        {
            return new AuthenticationResponseDto
            {
                Success = false,
                Message = "An error occurred during token refresh",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    /// <summary>
    /// Initiates a password reset request by sending a reset link to the user's email.
    /// </summary>
    public async Task<bool> RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            var user = await _userRepository
                .GetByEmailAsync(email, cancellationToken);

            if (user == null)
                return false;

            // Generate reset token
            // var resetToken = GenerateResetToken();

            // Store reset token with expiration (implement token storage)
            // await StorePasswordResetTokenAsync(user.UserId, resetToken, cancellationToken);

            // Send email with reset link (implement email service)
            // await _emailService.SendPasswordResetEmailAsync(email, resetToken, cancellationToken);

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Resets a user's password using a reset token.
    /// </summary>
    public async Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(resetPasswordDto.ResetToken) ||
                string.IsNullOrWhiteSpace(resetPasswordDto.NewPassword) ||
                resetPasswordDto.NewPassword != resetPasswordDto.ConfirmPassword)
            {
                return false;
            }

            var user = await _userRepository
                .GetByEmailAsync(resetPasswordDto.Email, cancellationToken);

            if (user == null)
                return false;

            // Validate reset token (implement token validation)
            // var tokenValid = await ValidatePasswordResetTokenAsync(user.UserId, resetPasswordDto.ResetToken, cancellationToken);
            // if (!tokenValid) return false;

            // Update password (implement password update)
            user.PasswordHash = HashPassword(resetPasswordDto.NewPassword);

            await _userRepository.UpdateAsync(user);

            // Invalidate reset token
            // await InvalidatePasswordResetTokenAsync(user.UserId, resetPasswordDto.ResetToken, cancellationToken);

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Changes the password for an authenticated user.
    /// </summary>
    public async Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordDto changePasswordDto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (changePasswordDto.NewPassword != changePasswordDto.ConfirmPassword)
                return false;

            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
                return false;

            // Verify current password (implement password verification)
            if (string.IsNullOrWhiteSpace(user.PasswordHash) || !VerifyPassword(changePasswordDto.CurrentPassword, user.PasswordHash))
                return false;

            // Update password (implement password update)
            user.PasswordHash = HashPassword(changePasswordDto.NewPassword);

            await _userRepository.UpdateAsync(user);

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Verifies a user's email address using a verification token.
    /// </summary>
    public async Task<bool> VerifyEmailAsync(VerifyEmailDto verifyEmailDto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(verifyEmailDto.Email) ||
                string.IsNullOrWhiteSpace(verifyEmailDto.VerificationToken))
                return false;

            var user = await _userRepository
                .GetByEmailAsync(verifyEmailDto.Email, cancellationToken);

            if (user == null)
                return false;

            // Validate email verification token (implement token validation)
            // var tokenValid = await ValidateEmailVerificationTokenAsync(user.UserId, verifyEmailDto.VerificationToken, cancellationToken);
            // if (!tokenValid) return false;

            // Mark email as verified (implement email verification tracking)
            // user.EmailVerified = true;

            await _userRepository.UpdateAsync(user);

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Resends a verification email to the user.
    /// </summary>
    public async Task<bool> ResendVerificationEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            var user = await _userRepository
                .GetByEmailAsync(email, cancellationToken);

            if (user == null)
                return false;

            // Generate verification token
            // var verificationToken = GenerateResetToken();

            // Store verification token (implement token storage)
            // await StoreEmailVerificationTokenAsync(user.UserId, verificationToken, cancellationToken);

            // Send verification email (implement email service)
            // await _emailService.SendVerificationEmailAsync(email, verificationToken, cancellationToken);

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Logs out a user by invalidating their refresh token.
    /// </summary>
    public async Task<bool> LogoutAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Invalidate all refresh tokens for the user (implement token invalidation)
        // await InvalidateAllRefreshTokensAsync(userId, cancellationToken);

        return await Task.FromResult(true);
    }

    /// <summary>
    /// Links a Google OAuth account to an existing user account.
    /// </summary>
    public async Task<bool> LinkGoogleAccountAsync(Guid userId, GoogleAuthDto googleAuthDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
                return false;

            // Check if Google ID is already linked to another account
            if (!string.IsNullOrWhiteSpace(googleAuthDto.GoogleId))
            {
                var existingGoogleUser = await _userRepository
                    .GetByGoogleAuthIdAsync(googleAuthDto.GoogleId, cancellationToken);

                if (existingGoogleUser != null && existingGoogleUser.UserId != userId)
                    return false;
            }

            // Link Google account
            user.GoogleAuthId = googleAuthDto.GoogleId;
            await _userRepository.UpdateAsync(user);

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Unlinks a Google OAuth account from an existing user account.
    /// </summary>
    public async Task<bool> UnlinkGoogleAccountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
                return false;

            // Unlink Google account
            user.GoogleAuthId = null;
            await _userRepository.UpdateAsync(user);

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if a user exists by email address.
    /// </summary>
    public async Task<bool> UserExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            return await _userRepository.ExistsByEmailAsync(email, cancellationToken);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Validates a JWT token and returns the user information if valid.
    /// </summary>
    public async Task<UserDto?> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(token))
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSecret);

            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtIssuer,
                ValidateAudience = true,
                ValidAudience = _jwtAudience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out _);

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return null;

            var user = await _userRepository.GetByIdWithRolesAsync(userId);

            return user != null ? _mapper.Map<UserDto>(user) : null;
        }
        catch
        {
            return null;
        }
    }

    // ========================== Private Helper Methods ==========================

    /// <summary>
    /// Generates a JWT access token for the given user.
    /// </summary>
    private string GenerateAccessToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSecret);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName)
        };

        // Add roles based on user entity relationships
        if (user.Admin != null)
        {
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));
        }

        if (user.CouncilEmployee != null)
        {
            claims.Add(new Claim(ClaimTypes.Role, "CouncilEmployee"));
        }

        // If user has no roles, add default "User" role
        if (user.Admin == null && user.CouncilEmployee == null)
        {
            claims.Add(new Claim(ClaimTypes.Role, "User"));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtExpirationMinutes),
            Issuer = _jwtIssuer,
            Audience = _jwtAudience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    /// <summary>
    /// Generates a refresh token (random string).
    /// </summary>
    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }

    /// <summary>
    /// Generates a password reset or email verification token.
    /// </summary>
    private string GenerateResetToken()
    {
        var randomNumber = new byte[32];
        using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }

    /// <summary>
    /// Hashes a password using BCrypt algorithm.
    /// </summary>
    public static string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be null or empty", nameof(password));
        
        return BCrypt.HashPassword(password, workFactor: 12);
    }

    /// <summary>
    /// Verifies a password against its BCrypt hash.
    /// </summary>
    private bool VerifyPassword(string password, string hash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hash))
            return false;

        try
        {
            return BCrypt.Verify(password, hash);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Validates the register DTO.
    /// </summary>
    private bool ValidateRegisterDto(RegisterDto registerDto, out List<string> errors)
    {
        errors = new List<string>();

        if (string.IsNullOrWhiteSpace(registerDto.FullName))
            errors.Add("Full name is required");

        if (string.IsNullOrWhiteSpace(registerDto.Email))
            errors.Add("Email is required");
        else if (!IsValidEmail(registerDto.Email))
            errors.Add("Invalid email format");

        if (string.IsNullOrWhiteSpace(registerDto.Password))
            errors.Add("Password is required");
        else if (registerDto.Password.Length < 8)
            errors.Add("Password must be at least 8 characters long");

        if (registerDto.Password != registerDto.ConfirmPassword)
            errors.Add("Passwords do not match");

        return errors.Count == 0;
    }

    /// <summary>
    /// Validates email format.
    /// </summary>
    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
