using System.Net.Http.Json;
using Edificia.Application.Ai.Dtos;
using Edificia.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Edificia.Infrastructure.Ai;

/// <summary>
/// AI service implementation that delegates text generation to an n8n webhook.
/// Replaces the former FluxAiService — no token management, no prompt construction.
/// The n8n workflow handles model selection, prompt building, and provider auth.
/// </summary>
public sealed class N8nAiService : IAiService
{
    private readonly HttpClient _httpClient;
    private readonly N8nAiSettings _settings;
    private readonly ILogger<N8nAiService> _logger;

    public N8nAiService(
        HttpClient httpClient,
        IOptions<N8nAiSettings> settings,
        ILogger<N8nAiService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<string?> GenerateTextAsync(
        AiGenerationRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Sending AI generation request to n8n webhook for section {SectionCode}",
            request.SectionCode);

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, _settings.WebhookUrl);
        httpRequest.Headers.Add("X-Edificia-Auth", _settings.ApiSecret);
        httpRequest.Content = JsonContent.Create(request);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<N8nAiResponse>(cancellationToken);

        if (result?.Usage is not null)
        {
            _logger.LogInformation(
                "n8n AI response received — Model: {Model}, Tokens: {Tokens}",
                result.Usage.Model ?? "unknown",
                result.Usage.Tokens ?? 0);
        }

        return result?.GeneratedText;
    }
}
