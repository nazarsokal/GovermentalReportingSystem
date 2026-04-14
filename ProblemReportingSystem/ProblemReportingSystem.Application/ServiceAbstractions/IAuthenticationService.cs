namespace ProblemReportingSystem.Application.ServiceAbstractions;

using ProblemReportingSystem.Application.DTOs;

/// <summary>
/// Service abstraction for handling user authentication and authorization operations.
/// Supports both traditional email/password authentication and Google OAuth 2.0 authentication.
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Registers a new user with email and password.
    /// </summary>
    /// <param name="registerDto">Registration details containing email, full name, and password</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authentication response with JWT token and user details</returns>
    Task<AuthenticationResponseDto> RegisterAsync(RegisterDto registerDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Authenticates a user with email and password.
    /// </summary>
    /// <param name="loginDto">Login credentials containing email and password</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authentication response with JWT token and user details</returns>
    Task<AuthenticationResponseDto> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Authenticates a user via Google OAuth 2.0 token.
    /// Creates a new user if one doesn't exist with the provided Google ID and email.
    /// </summary>
    /// <param name="googleAuthDto">Google authentication token and metadata</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authentication response with JWT token and user details</returns>
    Task<AuthenticationResponseDto> GoogleAuthenticateAsync(GoogleAuthDto googleAuthDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers a new user via Google OAuth 2.0.
    /// </summary>
    /// <param name="googleAuthDto">Google authentication token and metadata</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authentication response with JWT token and newly created user details</returns>
    Task<AuthenticationResponseDto> GoogleRegisterAsync(GoogleAuthDto googleAuthDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes an expired JWT access token using a refresh token.
    /// </summary>
    /// <param name="refreshTokenDto">Contains the refresh token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authentication response with new JWT token</returns>
    Task<AuthenticationResponseDto> RefreshTokenAsync(RefreshTokenDto refreshTokenDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Initiates a password reset request by sending a reset link to the user's email.
    /// </summary>
    /// <param name="email">Email address of the user requesting password reset</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Boolean indicating if the reset email was sent successfully</returns>
    Task<bool> RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resets a user's password using a reset token.
    /// </summary>
    /// <param name="resetPasswordDto">Contains reset token and new password</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Boolean indicating if the password was reset successfully</returns>
    Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Changes the password for an authenticated user.
    /// </summary>
    /// <param name="userId">ID of the user changing password</param>
    /// <param name="changePasswordDto">Contains current password and new password</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Boolean indicating if the password was changed successfully</returns>
    Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordDto changePasswordDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifies a user's email address using a verification token.
    /// </summary>
    /// <param name="verifyEmailDto">Contains email and verification token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Boolean indicating if email was verified successfully</returns>
    Task<bool> VerifyEmailAsync(VerifyEmailDto verifyEmailDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resends a verification email to the user.
    /// </summary>
    /// <param name="email">Email address to send verification to</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Boolean indicating if the verification email was sent successfully</returns>
    Task<bool> ResendVerificationEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs out a user by invalidating their refresh token.
    /// </summary>
    /// <param name="userId">ID of the user to log out</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Boolean indicating if logout was successful</returns>
    Task<bool> LogoutAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Links a Google OAuth account to an existing user account.
    /// </summary>
    /// <param name="userId">ID of the user to link Google account to</param>
    /// <param name="googleAuthDto">Google authentication token and metadata</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Boolean indicating if the Google account was linked successfully</returns>
    Task<bool> LinkGoogleAccountAsync(Guid userId, GoogleAuthDto googleAuthDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Unlinks a Google OAuth account from an existing user account.
    /// </summary>
    /// <param name="userId">ID of the user to unlink Google account from</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Boolean indicating if the Google account was unlinked successfully</returns>
    Task<bool> UnlinkGoogleAccountAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user exists by email address.
    /// </summary>
    /// <param name="email">Email address to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Boolean indicating if user with given email exists</returns>
    Task<bool> UserExistsByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a JWT token and returns the user information if valid.
    /// </summary>
    /// <param name="token">JWT token to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User details if token is valid, null otherwise</returns>
    Task<UserDto?> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);
}