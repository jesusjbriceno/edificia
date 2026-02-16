using System.Net.Http.Json;
using Edificia.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Edificia.Infrastructure.Email;

/// <summary>
/// Email service implementation using Brevo (formerly Sendinblue) transactional API.
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

        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"{_settings.BrevoApiUrl}/smtp/email",
                payload,
                cancellationToken);

            response.EnsureSuccessStatusCode();
            _logger.LogInformation("Email sent via Brevo to {Recipient}, subject: {Subject}", to, subject);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to send email via Brevo to {Recipient}", to);
            throw;
        }
    }
}
