namespace Edificia.Application.Interfaces;

/// <summary>
/// Service for sending emails. Supports SMTP and external providers (e.g., Brevo).
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an email to the specified recipient.
    /// </summary>
    /// <param name="to">Recipient email address.</param>
    /// <param name="subject">Email subject.</param>
    /// <param name="htmlBody">HTML body content.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default);
}
