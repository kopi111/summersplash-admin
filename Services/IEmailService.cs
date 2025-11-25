namespace SummerSplashWeb.Services;

public interface IEmailService
{
    Task SendVerificationEmailAsync(string toEmail, string verificationCode, string userName);
    Task SendPasswordResetEmailAsync(string toEmail, string resetToken, string userName);
    Task SendWelcomeEmailAsync(string toEmail, string userName);
}
