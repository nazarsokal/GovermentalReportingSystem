using Microsoft.AspNetCore.Authentication;

namespace ProblemReportingSystem.API.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProblemReportingSystem.Application.DTOs;
using ProblemReportingSystem.Application.ServiceAbstractions;

/// <summary>
/// API controller for authentication and user registration operations.
/// Provides endpoints for user registration, login, Google OAuth, token refresh, and password management.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthenticationService authenticationService, ILogger<AuthController> logger)
    {
        _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Registers a new user with email and password.
    /// </summary>
    /// <param name="registerDto">Registration details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authentication response with JWT token</returns>
    /// <response code="200">User successfully registered</response>
    /// <response code="400">Registration validation failed</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthenticationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthenticationResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            var response = new AuthenticationResponseDto
            {
                Success = false,
                Message = "Validation failed",
                Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
            };
            return BadRequest(response);
        }

        var result = await _authenticationService.RegisterAsync(registerDto, cancellationToken);

        if (!result.Success)
        {
            _logger.LogWarning($"Registration failed for email: {registerDto.Email}");
            return BadRequest(result);
        }

        _logger.LogInformation($"User registered successfully: {registerDto.Email}");
        return Ok(result);
    }

    /// <summary>
    /// Authenticates a user with email and password.
    /// </summary>
    /// <param name="loginDto">Login credentials</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authentication response with JWT token</returns>
    /// <response code="200">User successfully authenticated</response>
    /// <response code="400">Invalid credentials</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthenticationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthenticationResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new AuthenticationResponseDto
            {
                Success = false,
                Message = "Validation failed",
                Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
            });
        }

        var result = await _authenticationService.LoginAsync(loginDto, cancellationToken);

        if (!result.Success)
        {
            _logger.LogWarning($"Login failed for email: {loginDto.Email}");
            return BadRequest(result);
        }

        _logger.LogInformation($"User logged in successfully: {loginDto.Email}");
        return Ok(result);
    }

    /// <summary>
    /// Authenticates a user via Google OAuth 2.0.
    /// If user doesn't exist, creates a new user account.
    /// </summary>
    /// <param name="googleAuthDto">Google authentication data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authentication response with JWT token</returns>
    /// <response code="200">User successfully authenticated via Google</response>
    /// <response code="400">Google authentication failed</response>
    [HttpPost("google-authenticate")]
    [ProducesResponseType(typeof(AuthenticationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthenticationResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GoogleAuthenticate([FromBody] GoogleAuthDto googleAuthDto, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new AuthenticationResponseDto
            {
                Success = false,
                Message = "Validation failed",
                Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
            });
        }

        var result = await _authenticationService.GoogleAuthenticateAsync(googleAuthDto, cancellationToken);

        if (!result.Success)
        {
            _logger.LogWarning($"Google authentication failed for Google ID: {googleAuthDto.GoogleId}");
            return BadRequest(result);
        }

        _logger.LogInformation($"User authenticated via Google: {googleAuthDto.Email}");
        return Ok(result);
    }

    /// <summary>
    /// Registers a new user via Google OAuth 2.0.
    /// Fails if email is already registered.
    /// </summary>
    /// <param name="googleAuthDto">Google authentication data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authentication response with JWT token</returns>
    /// <response code="200">User successfully registered via Google</response>
    /// <response code="400">Google registration failed</response>
    [HttpPost("google-register")]
    [ProducesResponseType(typeof(AuthenticationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthenticationResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GoogleRegister([FromBody] GoogleAuthDto googleAuthDto, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new AuthenticationResponseDto
            {
                Success = false,
                Message = "Validation failed",
                Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
            });
        }

        var result = await _authenticationService.GoogleRegisterAsync(googleAuthDto, cancellationToken);

        if (!result.Success)
        {
            _logger.LogWarning($"Google registration failed for Google ID: {googleAuthDto.GoogleId}");
            return BadRequest(result);
        }

        _logger.LogInformation($"User registered via Google: {googleAuthDto.Email}");
        return Ok(result);
    }

    /// <summary>
    /// Refreshes an expired access token using a refresh token.
    /// </summary>
    /// <param name="refreshTokenDto">Refresh token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>New access token</returns>
    /// <response code="200">Token successfully refreshed</response>
    /// <response code="400">Token refresh failed</response>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(AuthenticationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthenticationResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new AuthenticationResponseDto
            {
                Success = false,
                Message = "Validation failed",
                Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
            });
        }

        var result = await _authenticationService.RefreshTokenAsync(refreshTokenDto, cancellationToken);

        if (!result.Success)
        {
            _logger.LogWarning("Token refresh failed");
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Requests a password reset email for the given email address.
    /// </summary>
    /// <param name="request">Email address for password reset</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success message</returns>
    /// <response code="200">Password reset email sent</response>
    /// <response code="400">Request failed</response>
    [HttpPost("request-password-reset")]
    [ProducesResponseType(typeof(PasswordResetResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PasswordResetResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RequestPasswordReset([FromBody] RequestPasswordResetDto request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new PasswordResetResponseDto
            {
                Success = false,
                Message = "Validation failed"
            });
        }

        var result = await _authenticationService.RequestPasswordResetAsync(request.Email, cancellationToken);

        if (!result)
        {
            _logger.LogWarning($"Password reset request failed for email: {request.Email}");
            return BadRequest(new PasswordResetResponseDto
            {
                Success = false,
                Message = "Failed to process password reset request"
            });
        }

        _logger.LogInformation($"Password reset email sent to: {request.Email}");
        return Ok(new PasswordResetResponseDto
        {
            Success = true,
            Message = "Password reset email has been sent to your email address"
        });
    }

    /// <summary>
    /// Resets a user's password using a reset token.
    /// </summary>
    /// <param name="resetPasswordDto">Reset token and new password</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success message</returns>
    /// <response code="200">Password successfully reset</response>
    /// <response code="400">Password reset failed</response>
    [HttpPost("reset-password")]
    [ProducesResponseType(typeof(PasswordResetResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PasswordResetResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new PasswordResetResponseDto
            {
                Success = false,
                Message = "Validation failed"
            });
        }

        var result = await _authenticationService.ResetPasswordAsync(resetPasswordDto, cancellationToken);

        if (!result)
        {
            _logger.LogWarning($"Password reset failed for email: {resetPasswordDto.Email}");
            return BadRequest(new PasswordResetResponseDto
            {
                Success = false,
                Message = "Failed to reset password"
            });
        }

        _logger.LogInformation($"Password reset successfully for email: {resetPasswordDto.Email}");
        return Ok(new PasswordResetResponseDto
        {
            Success = true,
            Message = "Password has been reset successfully"
        });
    }

    /// <summary>
    /// Changes the password for an authenticated user.
    /// Requires the current password for verification.
    /// </summary>
    /// <param name="changePasswordDto">Current and new password</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success message</returns>
    /// <response code="200">Password successfully changed</response>
    /// <response code="400">Password change failed</response>
    /// <response code="401">Unauthorized</response>
    [Authorize]
    [HttpPost("change-password")]
    [ProducesResponseType(typeof(PasswordResetResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PasswordResetResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new PasswordResetResponseDto
            {
                Success = false,
                Message = "Validation failed"
            });
        }

        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized();
        }

        var result = await _authenticationService.ChangePasswordAsync(userId, changePasswordDto, cancellationToken);

        if (!result)
        {
            _logger.LogWarning($"Password change failed for user: {userId}");
            return BadRequest(new PasswordResetResponseDto
            {
                Success = false,
                Message = "Failed to change password"
            });
        }

        _logger.LogInformation($"Password changed successfully for user: {userId}");
        return Ok(new PasswordResetResponseDto
        {
            Success = true,
            Message = "Password has been changed successfully"
        });
    }

    /// <summary>
    /// Verifies a user's email address using a verification token.
    /// </summary>
    /// <param name="verifyEmailDto">Email and verification token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Email verification result</returns>
    /// <response code="200">Email successfully verified</response>
    /// <response code="400">Email verification failed</response>
    [HttpPost("verify-email")]
    [ProducesResponseType(typeof(EmailVerificationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EmailVerificationResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto verifyEmailDto, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new EmailVerificationResponseDto
            {
                Success = false,
                Message = "Validation failed"
            });
        }

        var result = await _authenticationService.VerifyEmailAsync(verifyEmailDto, cancellationToken);

        if (!result)
        {
            _logger.LogWarning($"Email verification failed for: {verifyEmailDto.Email}");
            return BadRequest(new EmailVerificationResponseDto
            {
                Success = false,
                Message = "Email verification failed"
            });
        }

        _logger.LogInformation($"Email verified successfully: {verifyEmailDto.Email}");
        return Ok(new EmailVerificationResponseDto
        {
            Success = true,
            Message = "Email has been verified successfully"
        });
    }

    /// <summary>
    /// Resends a verification email to the user.
    /// </summary>
    /// <param name="request">User's email address</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success message</returns>
    /// <response code="200">Verification email sent</response>
    /// <response code="400">Request failed</response>
    [HttpPost("resend-verification-email")]
    [ProducesResponseType(typeof(EmailVerificationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EmailVerificationResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResendVerificationEmail([FromBody] RequestPasswordResetDto request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new EmailVerificationResponseDto
            {
                Success = false,
                Message = "Validation failed"
            });
        }

        var result = await _authenticationService.ResendVerificationEmailAsync(request.Email, cancellationToken);

        if (!result)
        {
            _logger.LogWarning($"Resend verification email failed for: {request.Email}");
            return BadRequest(new EmailVerificationResponseDto
            {
                Success = false,
                Message = "Failed to resend verification email"
            });
        }

        _logger.LogInformation($"Verification email resent to: {request.Email}");
        return Ok(new EmailVerificationResponseDto
        {
            Success = true,
            Message = "Verification email has been sent to your email address"
        });
    }

    /// <summary>
    /// Logs out a user by invalidating their refresh token.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success message</returns>
    /// <response code="200">User successfully logged out</response>
    /// <response code="401">Unauthorized</response>
    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(typeof(PasswordResetResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken = default)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized();
        }

        var result = await _authenticationService.LogoutAsync(userId, cancellationToken);

        if (!result)
        {
            _logger.LogWarning($"Logout failed for user: {userId}");
            return BadRequest(new PasswordResetResponseDto
            {
                Success = false,
                Message = "Failed to logout"
            });
        }

        _logger.LogInformation($"User logged out successfully: {userId}");
        return Ok(new PasswordResetResponseDto
        {
            Success = true,
            Message = "You have been logged out successfully"
        });
    }

    /// <summary>
    /// Links a Google OAuth account to an existing user account.
    /// </summary>
    /// <param name="googleAuthDto">Google authentication data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success message</returns>
    /// <response code="200">Google account successfully linked</response>
    /// <response code="400">Linking failed</response>
    /// <response code="401">Unauthorized</response>
    [Authorize]
    [HttpPost("link-google-account")]
    [ProducesResponseType(typeof(PasswordResetResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PasswordResetResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LinkGoogleAccount([FromBody] GoogleAuthDto googleAuthDto, CancellationToken cancellationToken = default)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized();
        }

        var result = await _authenticationService.LinkGoogleAccountAsync(userId, googleAuthDto, cancellationToken);

        if (!result)
        {
            _logger.LogWarning($"Google account linking failed for user: {userId}");
            return BadRequest(new PasswordResetResponseDto
            {
                Success = false,
                Message = "Failed to link Google account"
            });
        }

        _logger.LogInformation($"Google account linked successfully for user: {userId}");
        return Ok(new PasswordResetResponseDto
        {
            Success = true,
            Message = "Google account has been linked successfully"
        });
    }

    /// <summary>
    /// Unlinks a Google OAuth account from an existing user account.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success message</returns>
    /// <response code="200">Google account successfully unlinked</response>
    /// <response code="400">Unlinking failed</response>
    /// <response code="401">Unauthorized</response>
    [Authorize]
    [HttpPost("unlink-google-account")]
    [ProducesResponseType(typeof(PasswordResetResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PasswordResetResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UnlinkGoogleAccount(CancellationToken cancellationToken = default)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized();
        }

        var result = await _authenticationService.UnlinkGoogleAccountAsync(userId, cancellationToken);

        if (!result)
        {
            _logger.LogWarning($"Google account unlinking failed for user: {userId}");
            return BadRequest(new PasswordResetResponseDto
            {
                Success = false,
                Message = "Failed to unlink Google account"
            });
        }

        _logger.LogInformation($"Google account unlinked successfully for user: {userId}");
        return Ok(new PasswordResetResponseDto
        {
            Success = true,
            Message = "Google account has been unlinked successfully"
        });
    }

    /// <summary>
    /// Checks if an email address is available for registration.
    /// </summary>
    /// <param name="email">Email address to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Email availability status</returns>
    /// <response code="200">Email availability checked</response>
    [HttpGet("check-email-availability")]
    [ProducesResponseType(typeof(EmailAvailabilityDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckEmailAvailability([FromQuery] string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest(new EmailAvailabilityDto
            {
                Email = email,
                IsAvailable = false
            });
        }

        var exists = await _authenticationService.UserExistsByEmailAsync(email, cancellationToken);

        return Ok(new EmailAvailabilityDto
        {
            Email = email,
            IsAvailable = !exists
        });
    }

    /// <summary>
    /// Validates a JWT token and returns the authenticated user information.
    /// </summary>
    /// <param name="token">JWT token to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User information if token is valid</returns>
    /// <response code="200">Token is valid</response>
    /// <response code="401">Token is invalid or expired</response>
    [HttpPost("validate-token")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ValidateToken([FromBody] string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return Unauthorized();
        }

        var user = await _authenticationService.ValidateTokenAsync(token, cancellationToken);

        if (user == null)
        {
            return Unauthorized();
        }

        return Ok(user);
    }
}

