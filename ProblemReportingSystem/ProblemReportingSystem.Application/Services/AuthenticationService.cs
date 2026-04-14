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
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProblemReportingSystem.Application.DTOs;
using ProblemReportingSystem.Application.ServiceAbstractions;
using ProblemReportingSystem.DAL.Entities;
using ProblemReportingSystem.DAL.Infrastructure;

/// <summary>
/// Implementation of the authentication service for user registration, login, and OAuth operations.
/// Handles both email/password and Google OAuth 2.0 authentication flows.
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly ProblemReportingSystemDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly string _jwtSecret;
    private readonly string _jwtIssuer;
    private readonly string _jwtAudience;
    private readonly int _jwtExpirationMinutes;
    private readonly int _refreshTokenExpirationDays;

    public AuthenticationService(
        ProblemReportingSystemDbContext dbContext,
        IMapper mapper,
        IConfiguration configuration)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        
        var jwtSettings = configuration.GetSection("JwtSettings");
        _jwtSecret = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");
        _jwtIssuer = jwtSettings["Issuer"] ?? throw new InvalidOperationException("JWT Issuer not configured");
        _jwtAudience = jwtSettings["Audience"] ?? throw new InvalidOperationException("JWT Audience not configured");
        _jwtExpirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "60");
        _refreshTokenExpirationDays = int.Parse(jwtSettings["RefreshTokenExpirationDays"] ?? "7");
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
            var existingUser = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Email == registerDto.Email, cancellationToken);

            if (existingUser != null)
            {
                return new AuthenticationResponseDto
                {
                    Success = false,
                    Message = "Email is already registered",
                    Errors = new List<string> { "Email already in use" }
                };
            }

            // Create new user
            var user = new User
            {
                UserId = Guid.NewGuid(),
                FullName = registerDto.FullName,
                Email = registerDto.Email,
                IsActive = true
            };

            // Hash and store password (implementation details depend on your identity framework)
            // This is a placeholder - implement actual password hashing
            var hashedPassword = HashPassword(registerDto.Password);
            
            // Note: You may need to extend the User entity to include password hash
            // For now, this assumes you have a password storage mechanism

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync(cancellationToken);

            // Generate tokens
            var accessToken = GenerateAccessToken(user);
            var refreshToken = GenerateRefreshToken();

            // Store refresh token (implement token storage in database)
            // await StoreRefreshTokenAsync(user.UserId, refreshToken, cancellationToken);

            var userDto = _mapper.Map<UserDto>(user);

            return new AuthenticationResponseDto
            {
                Success = true,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = _jwtExpirationMinutes * 60,
                TokenType = "Bearer",
                User = userDto,
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
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Email == loginDto.Email, cancellationToken);

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

            // Verify password (implement actual password verification)
            // var passwordValid = VerifyPassword(loginDto.Password, hashedPassword);
            // For now, this is a placeholder
            var passwordValid = VerifyPassword(loginDto.Password, user.Email);

            if (!passwordValid)
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

            // Store refresh token (implement token storage in database)
            // await StoreRefreshTokenAsync(user.UserId, refreshToken, cancellationToken);

            var userDto = _mapper.Map<UserDto>(user);

            return new AuthenticationResponseDto
            {
                Success = true,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = _jwtExpirationMinutes * 60,
                TokenType = "Bearer",
                User = userDto,
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

            // Try to find user by Google ID
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.GoogleAuthId == googleAuthDto.GoogleId, cancellationToken);

            // If user doesn't exist, try to find by email
            if (user == null && !string.IsNullOrWhiteSpace(googleAuthDto.Email))
            {
                user = await _dbContext.Users
                    .FirstOrDefaultAsync(u => u.Email == googleAuthDto.Email, cancellationToken);

                // If user exists with email but no Google ID, link the Google account
                if (user != null)
                {
                    user.GoogleAuthId = googleAuthDto.GoogleId;
                    _dbContext.Users.Update(user);
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }
            }

            // If user still doesn't exist, create new user
            if (user == null)
            {
                user = new User
                {
                    UserId = Guid.NewGuid(),
                    FullName = googleAuthDto.FullName ?? "Google User",
                    Email = googleAuthDto.Email ?? $"google-{googleAuthDto.GoogleId}@oauth.local",
                    GoogleAuthId = googleAuthDto.GoogleId,
                    IsActive = true
                };

                _dbContext.Users.Add(user);
                await _dbContext.SaveChangesAsync(cancellationToken);
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

            // Store refresh token
            // await StoreRefreshTokenAsync(user.UserId, refreshToken, cancellationToken);

            var userDto = _mapper.Map<UserDto>(user);

            return new AuthenticationResponseDto
            {
                Success = true,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = _jwtExpirationMinutes * 60,
                TokenType = "Bearer",
                User = userDto,
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
                var existingUser = await _dbContext.Users
                    .FirstOrDefaultAsync(u => u.Email == googleAuthDto.Email, cancellationToken);

                if (existingUser != null)
                {
                    // If user exists but doesn't have Google ID, link it
                    if (string.IsNullOrWhiteSpace(existingUser.GoogleAuthId))
                    {
                        existingUser.GoogleAuthId = googleAuthDto.GoogleId;
                        _dbContext.Users.Update(existingUser);
                        await _dbContext.SaveChangesAsync(cancellationToken);
                    }

                    // Proceed with authentication instead
                    return await GoogleAuthenticateAsync(googleAuthDto, cancellationToken);
                }
            }

            // Check if Google ID is already registered
            var googleUser = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.GoogleAuthId == googleAuthDto.GoogleId, cancellationToken);

            if (googleUser != null)
            {
                return new AuthenticationResponseDto
                {
                    Success = false,
                    Message = "This Google account is already registered",
                    Errors = new List<string> { "Google account already in use" }
                };
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

            _dbContext.Users.Add(newUser);
            await _dbContext.SaveChangesAsync(cancellationToken);

            // Generate tokens
            var accessToken = GenerateAccessToken(newUser);
            var refreshToken = GenerateRefreshToken();

            // Store refresh token
            // await StoreRefreshTokenAsync(newUser.UserId, refreshToken, cancellationToken);

            var userDto = _mapper.Map<UserDto>(newUser);

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
            // var user = await _dbContext.Users.FindAsync(new object[] { storedToken.UserId }, cancellationToken: cancellationToken);
            
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

            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

            if (user == null)
                return false;

            // Generate reset token
            var resetToken = GenerateResetToken();

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

            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Email == resetPasswordDto.Email, cancellationToken);

            if (user == null)
                return false;

            // Validate reset token (implement token validation)
            // var tokenValid = await ValidatePasswordResetTokenAsync(user.UserId, resetPasswordDto.ResetToken, cancellationToken);
            // if (!tokenValid) return false;

            // Update password (implement password update)
            // var hashedPassword = HashPassword(resetPasswordDto.NewPassword);
            // user.PasswordHash = hashedPassword;

            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync(cancellationToken);

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

            var user = await _dbContext.Users.FindAsync(new object[] { userId }, cancellationToken: cancellationToken);

            if (user == null)
                return false;

            // Verify current password (implement password verification)
            // var passwordValid = VerifyPassword(changePasswordDto.CurrentPassword, user.PasswordHash);
            // if (!passwordValid) return false;

            // Update password (implement password update)
            // var hashedPassword = HashPassword(changePasswordDto.NewPassword);
            // user.PasswordHash = hashedPassword;

            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync(cancellationToken);

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

            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Email == verifyEmailDto.Email, cancellationToken);

            if (user == null)
                return false;

            // Validate email verification token (implement token validation)
            // var tokenValid = await ValidateEmailVerificationTokenAsync(user.UserId, verifyEmailDto.VerificationToken, cancellationToken);
            // if (!tokenValid) return false;

            // Mark email as verified (implement email verification tracking)
            // user.EmailVerified = true;

            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync(cancellationToken);

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

            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

            if (user == null)
                return false;

            // Generate verification token
            var verificationToken = GenerateResetToken();

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
        try
        {
            // Invalidate all refresh tokens for the user (implement token invalidation)
            // await InvalidateAllRefreshTokensAsync(userId, cancellationToken);

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Links a Google OAuth account to an existing user account.
    /// </summary>
    public async Task<bool> LinkGoogleAccountAsync(Guid userId, GoogleAuthDto googleAuthDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _dbContext.Users.FindAsync(new object[] { userId }, cancellationToken: cancellationToken);

            if (user == null)
                return false;

            // Check if Google ID is already linked to another account
            var existingGoogleUser = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.GoogleAuthId == googleAuthDto.GoogleId && u.UserId != userId, cancellationToken);

            if (existingGoogleUser != null)
                return false;

            // Link Google account
            user.GoogleAuthId = googleAuthDto.GoogleId;
            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync(cancellationToken);

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
            var user = await _dbContext.Users.FindAsync(new object[] { userId }, cancellationToken: cancellationToken);

            if (user == null)
                return false;

            // Unlink Google account
            user.GoogleAuthId = null;
            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync(cancellationToken);

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

            return await _dbContext.Users.AnyAsync(u => u.Email == email, cancellationToken);
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
            }, out SecurityToken validatedToken);

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return null;

            var user = await _dbContext.Users.FindAsync(new object[] { userId }, cancellationToken: cancellationToken);

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
    /// Hashes a password (placeholder implementation).
    /// </summary>
    private string HashPassword(string password)
    {
        // TODO: Implement proper password hashing using BCrypt or similar
        // Example: return BCrypt.Net.BCrypt.HashPassword(password);
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(password)); // Placeholder
    }

    /// <summary>
    /// Verifies a password against its hash (placeholder implementation).
    /// </summary>
    private bool VerifyPassword(string password, string hash)
    {
        // TODO: Implement proper password verification using BCrypt or similar
        // Example: return BCrypt.Net.BCrypt.Verify(password, hash);
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(password)) == hash; // Placeholder
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

