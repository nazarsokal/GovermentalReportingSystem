namespace ProblemReportingSystem.Application.DTOs;

/// <summary>
/// DTO for user registration with email and password.
/// </summary>
public class RegisterDto
{
    /// <summary>
    /// User's full name
    /// </summary>
    public string FullName { get; set; } = null!;

    /// <summary>
    /// User's email address (must be unique)
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// User's password (will be hashed before storage)
    /// </summary>
    public string Password { get; set; } = null!;

    /// <summary>
    /// Confirmation password (must match Password)
    /// </summary>
    public string ConfirmPassword { get; set; } = null!;

    /// <summary>
    /// User's district
    /// </summary>
    public string? District { get; set; }
    
    public string? City { get; set; }

    /// <summary>
    /// User's oblast (region)
    /// </summary>
    public string? Oblast { get; set; }
}

/// <summary>
/// DTO for user login with email and password.
/// </summary>
public class LoginDto
{
    /// <summary>
    /// User's email address
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// User's password
    /// </summary>
    public string Password { get; set; } = null!;

    /// <summary>
    /// Optional: Remember me flag for extended session duration
    /// </summary>
    public bool RememberMe { get; set; }
}

/// <summary>
/// DTO for Google OAuth 2.0 authentication and registration.
/// </summary>
public class GoogleAuthDto
{
    /// <summary>
    /// Google ID token received from Google's OAuth provider
    /// </summary>
    public string IdToken { get; set; } = null!;

    /// <summary>
    /// User's full name from Google profile
    /// </summary>
    public string? FullName { get; set; }

    /// <summary>
    /// User's email from Google profile
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Google's unique user identifier
    /// </summary>
    public string? GoogleId { get; set; }

    /// <summary>
    /// User's profile picture URL from Google
    /// </summary>
    public string? ProfilePictureUrl { get; set; }
}

/// <summary>
/// DTO for refreshing an expired access token.
/// </summary>
public class RefreshTokenDto
{
    /// <summary>
    /// The refresh token issued during authentication
    /// </summary>
    public string RefreshToken { get; set; } = null!;
}

/// <summary>
/// DTO for requesting a password reset.
/// </summary>
public class RequestPasswordResetDto
{
    /// <summary>
    /// User's email address
    /// </summary>
    public string Email { get; set; } = null!;
}

/// <summary>
/// DTO for resetting a password with a reset token.
/// </summary>
public class ResetPasswordDto
{
    /// <summary>
    /// The reset token sent to the user's email
    /// </summary>
    public string ResetToken { get; set; } = null!;

    /// <summary>
    /// The new password
    /// </summary>
    public string NewPassword { get; set; } = null!;

    /// <summary>
    /// Confirmation of the new password
    /// </summary>
    public string ConfirmPassword { get; set; } = null!;

    /// <summary>
    /// User's email address (for verification)
    /// </summary>
    public string Email { get; set; } = null!;
}

/// <summary>
/// DTO for changing password by an authenticated user.
/// </summary>
public class ChangePasswordDto
{
    /// <summary>
    /// User's current password
    /// </summary>
    public string CurrentPassword { get; set; } = null!;

    /// <summary>
    /// The new password
    /// </summary>
    public string NewPassword { get; set; } = null!;

    /// <summary>
    /// Confirmation of the new password
    /// </summary>
    public string ConfirmPassword { get; set; } = null!;
}

/// <summary>
/// DTO for verifying an email address.
/// </summary>
public class VerifyEmailDto
{
    /// <summary>
    /// User's email address
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// Email verification token
    /// </summary>
    public string VerificationToken { get; set; } = null!;
}

/// <summary>
/// Response DTO after successful authentication or registration.
/// </summary>
public class AuthenticationResponseDto
{
    /// <summary>
    /// Whether the authentication/registration was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// JWT access token for API authentication
    /// </summary>
    public string? AccessToken { get; set; }

    /// <summary>
    /// Refresh token for obtaining new access tokens
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// Expiration time of the access token (in seconds)
    /// </summary>
    public int? ExpiresIn { get; set; }

    /// <summary>
    /// Type of token (usually "Bearer")
    /// </summary>
    public string? TokenType { get; set; }

    /// <summary>
    /// Authenticated/registered user information
    /// </summary>
    public UserDto? User { get; set; }

    /// <summary>
    /// User's address information (returned on successful login/registration)
    /// </summary>
    public AddressDto? Address { get; set; }

    /// <summary>
    /// Council ID for council employees (null for other roles)
    /// </summary>
    public Guid? CouncilId { get; set; }

    /// <summary>
    /// Error message if authentication/registration failed
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// List of error messages (for validation errors)
    /// </summary>
    public List<string> Errors { get; set; } = new List<string>();
}

/// <summary>
/// Response DTO for email verification results.
/// </summary>
public class EmailVerificationResponseDto
{
    /// <summary>
    /// Whether email verification was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Message describing the verification result
    /// </summary>
    public string Message { get; set; } = null!;
}

/// <summary>
/// Response DTO for password reset request.
/// </summary>
public class PasswordResetResponseDto
{
    /// <summary>
    /// Whether the password reset request was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Message describing the result
    /// </summary>
    public string Message { get; set; } = null!;
}

/// <summary>
/// DTO for validating user existence by email.
/// </summary>
public class EmailAvailabilityDto
{
    /// <summary>
    /// Email address to check
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// Whether the email is available (not in use)
    /// </summary>
    public bool IsAvailable { get; set; }
}
