using System;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using SummerSplashWeb.Models;

namespace SummerSplashWeb.Services
{
    public class AuthService : IAuthService
    {
        private readonly IDatabaseService _databaseService;
        private readonly ILogger<AuthService> _logger;
        private readonly IEmailService _emailService;
        private User? _currentUser;

        public User? CurrentUser => _currentUser;
        public bool IsAuthenticated => _currentUser != null;

        public AuthService(IDatabaseService databaseService, ILogger<AuthService> logger, IEmailService emailService)
        {
            _databaseService = databaseService;
            _logger = logger;
            _emailService = emailService;
        }

        public async Task<User?> LoginAsync(string email, string password)
        {
            try
            {
                _logger.LogInformation("=== LOGIN ATTEMPT ===");
                _logger.LogInformation("Email: {Email}", email);
                _logger.LogInformation("Password length: {Length}", password?.Length ?? 0);

                using var connection = _databaseService.CreateConnection();

                var sql = @"
                    SELECT UserId, FirstName, LastName, Email, ProfilePicture, Position,
                           IsActive, IsApproved, EmailVerified, CreatedAt, LastLoginAt, PasswordHash, PasswordSalt
                    FROM Users
                    WHERE Email = @Email AND IsActive = 1";

                var user = await connection.QueryFirstOrDefaultAsync<dynamic>(sql, new { Email = email });

                if (user == null)
                {
                    _logger.LogWarning("User not found or not active for email: {Email}", email);
                    return null;
                }

                string userEmail = user.Email;
                bool isActive = user.IsActive;
                bool isApproved = user.IsApproved;
                string hash = user.PasswordHash;

                _logger.LogInformation("User found - Email: {Email}, IsActive: {IsActive}, IsApproved: {IsApproved}",
                    userEmail, isActive, isApproved);
                _logger.LogInformation("Stored hash: {Hash}", hash);
                _logger.LogInformation("Hash length: {Length}", hash?.Length ?? 0);

                // Verify password
                bool passwordValid = false;
                try
                {
                    passwordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
                    _logger.LogInformation("BCrypt.Verify result: {Result}", passwordValid);
                }
                catch (Exception bcryptEx)
                {
                    _logger.LogError(bcryptEx, "BCrypt verification error");
                    return null;
                }

                if (!passwordValid)
                {
                    _logger.LogWarning("Password verification failed for {Email}", email);
                    return null;
                }

                _logger.LogInformation("Password verified successfully for {Email}", email);

                // Check if email is verified
                bool emailVerified = user.EmailVerified;
                _logger.LogInformation("EmailVerified value: {EmailVerified}", emailVerified);
                if (!emailVerified)
                    throw new Exception("EMAIL_NOT_VERIFIED");

                // Check if user is approved
                if (!user.IsApproved)
                    throw new Exception("Your account is pending approval by an administrator.");

                // Update last login
                await connection.ExecuteAsync(
                    "UPDATE Users SET LastLoginAt = @Now WHERE UserId = @UserId",
                    new { Now = DateTime.Now, UserId = user.UserId }
                );

                _currentUser = new User
                {
                    UserId = user.UserId,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    ProfilePicture = user.ProfilePicture,
                    Position = user.Position,
                    IsActive = user.IsActive,
                    IsApproved = user.IsApproved,
                    EmailVerified = user.EmailVerified,
                    CreatedAt = user.CreatedAt,
                    LastLoginAt = DateTime.Now
                };

                return _currentUser;
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> RegisterAsync(string firstName, string lastName, string email, string password)
        {
            try
            {
                using var connection = _databaseService.CreateConnection();

                // Check if email already exists
                var exists = await connection.ExecuteScalarAsync<bool>(
                    "SELECT COUNT(1) FROM Users WHERE Email = @Email",
                    new { Email = email }
                );

                if (exists)
                    throw new Exception("Email already registered.");

                // Hash password
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
                var passwordSalt = BCrypt.Net.BCrypt.GenerateSalt();

                // Generate verification code
                var verificationCode = GenerateVerificationCode();
                var verificationExpiry = DateTime.Now.AddMinutes(15);

                var sql = @"
                    INSERT INTO Users (FirstName, LastName, Email, PasswordHash, PasswordSalt, IsActive, IsApproved, EmailVerified, VerificationCode, VerificationCodeExpiry)
                    VALUES (@FirstName, @LastName, @Email, @PasswordHash, @PasswordSalt, 1, 0, 0, @VerificationCode, @VerificationCodeExpiry)";

                var result = await connection.ExecuteAsync(sql, new
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email,
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                    VerificationCode = verificationCode,
                    VerificationCodeExpiry = verificationExpiry
                });

                if (result > 0)
                {
                    // Send verification email
                    _logger.LogInformation("Verification code for {Email}: {Code}", email, verificationCode);

                    try
                    {
                        await _emailService.SendVerificationEmailAsync(email, verificationCode, firstName);
                        _logger.LogInformation("Verification email sent to {Email}", email);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send verification email to {Email}", email);
                        // Don't fail registration if email fails
                    }
                }

                return result > 0;
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> RequestPasswordResetAsync(string email)
        {
            try
            {
                using var connection = _databaseService.CreateConnection();

                // Check if user exists
                var user = await connection.QueryFirstOrDefaultAsync<User>(
                    "SELECT * FROM Users WHERE Email = @Email",
                    new { Email = email }
                );

                if (user == null)
                    return false;

                // Generate reset token
                var token = Guid.NewGuid().ToString();
                var expiresAt = DateTime.Now.AddHours(24);

                var sql = @"
                    INSERT INTO PasswordResetTokens (UserId, Token, ExpiresAt)
                    VALUES (@UserId, @Token, @ExpiresAt)";

                await connection.ExecuteAsync(sql, new
                {
                    UserId = user.UserId,
                    Token = token,
                    ExpiresAt = expiresAt
                });

                // In a real application, send email with reset link
                // For now, we'll just log the token
                System.Diagnostics.Debug.WriteLine($"Password reset token for {email}: {token}");

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            try
            {
                using var connection = _databaseService.CreateConnection();

                // Verify token
                var resetToken = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    @"SELECT * FROM PasswordResetTokens
                      WHERE Token = @Token AND IsUsed = 0 AND ExpiresAt > @Now",
                    new { Token = token, Now = DateTime.Now }
                );

                if (resetToken == null)
                    return false;

                // Update password
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                var passwordSalt = BCrypt.Net.BCrypt.GenerateSalt();

                await connection.ExecuteAsync(
                    @"UPDATE Users
                      SET PasswordHash = @PasswordHash, PasswordSalt = @PasswordSalt
                      WHERE UserId = @UserId",
                    new { PasswordHash = passwordHash, PasswordSalt = passwordSalt, UserId = resetToken.UserId }
                );

                // Mark token as used
                await connection.ExecuteAsync(
                    "UPDATE PasswordResetTokens SET IsUsed = 1 WHERE TokenId = @TokenId",
                    new { TokenId = resetToken.TokenId }
                );

                return true;
            }
            catch
            {
                return false;
            }
        }

        public Task LogoutAsync()
        {
            _currentUser = null;
            return Task.CompletedTask;
        }

        public async Task<string> GenerateVerificationCodeAsync(string email)
        {
            try
            {
                using var connection = _databaseService.CreateConnection();

                var code = GenerateVerificationCode();
                var expiry = DateTime.Now.AddMinutes(15);

                await connection.ExecuteAsync(
                    @"UPDATE Users
                      SET VerificationCode = @Code, VerificationCodeExpiry = @Expiry
                      WHERE Email = @Email",
                    new { Code = code, Expiry = expiry, Email = email }
                );

                _logger.LogInformation("Verification code for {Email}: {Code}", email, code);
                return code;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating verification code");
                throw;
            }
        }

        public async Task<bool> VerifyCodeAsync(string email, string code)
        {
            try
            {
                using var connection = _databaseService.CreateConnection();

                var user = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    @"SELECT UserId, VerificationCode, VerificationCodeExpiry
                      FROM Users
                      WHERE Email = @Email",
                    new { Email = email }
                );

                if (user == null)
                    return false;

                // Check if code matches and not expired
                if (user.VerificationCode != code)
                {
                    _logger.LogWarning("Invalid verification code for {Email}", email);
                    return false;
                }

                if (user.VerificationCodeExpiry < DateTime.Now)
                {
                    _logger.LogWarning("Expired verification code for {Email}", email);
                    return false;
                }

                // Mark email as verified
                await connection.ExecuteAsync(
                    @"UPDATE Users
                      SET EmailVerified = 1, VerificationCode = NULL, VerificationCodeExpiry = NULL
                      WHERE UserId = @UserId",
                    new { UserId = user.UserId }
                );

                _logger.LogInformation("Email verified successfully for {Email}", email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying code");
                return false;
            }
        }

        public async Task<bool> SendVerificationCodeAsync(string email)
        {
            try
            {
                var code = await GenerateVerificationCodeAsync(email);
                // In production, send email with code
                // For now, just log it
                return !string.IsNullOrEmpty(code);
            }
            catch
            {
                return false;
            }
        }

        private string GenerateVerificationCode()
        {
            var random = new Random();
            var code = "";
            for (int i = 0; i < 7; i++)
            {
                code += random.Next(0, 10).ToString();
            }
            return code;
        }
    }
}
