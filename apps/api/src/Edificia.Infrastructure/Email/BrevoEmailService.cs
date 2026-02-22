using System.Net.Http.Json;
using System.Text.Json;
using Edificia.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Edificia.Infrastructure.Email;

/// <summary>
/// Email service implementation using Brevo (formerly Sendinblue) transactional API v3.
/// Endpoint: POST https://api.brevo.com/v3/smtp/email
/// Auth: Header "api-key" configured in DI via HttpClient.
/// </summary>
public sealed class BrevoEmailService : IEmailService
{
    private readonly HttpClient _httpClient;
    private readonly EmailSettings _settings;
    private readonly ILogger<BrevoEmailService> _logger;

    public BrevoEmailService(
        HttpClient httpClient,
        IOptions<EmailSettings> settings,
        ILogger<BrevoEmailService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendAsync(
        string to,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            sender = new { email = _settings.FromAddress, name = _settings.FromName },
            to = new[] { new { email = to } },
            subject,
            htmlContent = htmlBody
        };

        var url = $"{_settings.BrevoApiUrl}/smtp/email";

        _logger.LogDebug(
            "Sending email via Brevo API to {Recipient}, from {Sender}, subject: {Subject}",
            to, _settings.FromAddress, subject);

        try
        {
            var response = await _httpClient.PostAsJsonAsync(url, payload, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError(
                    "Brevo API returned {StatusCode} for email to {Recipient}: {ErrorBody}",
                    (int)response.StatusCode, to, errorBody);
                response.EnsureSuccessStatusCode(); // throws HttpRequestException
            }

            _logger.LogInformation(
                "Email sent via Brevo to {Recipient}, subject: {Subject}", to, subject);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to send email via Brevo to {Recipient}", to);
            throw;
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(ex, "Brevo API request timed out for email to {Recipient}", to);
            throw;
        }
    }
}
