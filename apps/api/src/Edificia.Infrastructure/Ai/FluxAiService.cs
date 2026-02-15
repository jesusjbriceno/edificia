using System.Net.Http.Headers;
using System.Net.Http.Json;
using Edificia.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Edificia.Infrastructure.Ai;

/// <summary>
/// Implementation of <see cref="IAiService"/> using the Flux AI Gateway.
/// Manages OAuth2 token lifecycle with in-memory caching.
/// </summary>
public sealed class FluxAiService : IAiService
{
    private const string TokenCacheKey = "flux_gateway_token";
    private static readonly TimeSpan TokenCacheDuration = TimeSpan.FromMinutes(50);

    private readonly HttpClient _httpClient;
    private readonly FluxGatewaySettings _settings;
    private readonly IMemoryCache _cache;
    private readonly ILogger<FluxAiService> _logger;

    public FluxAiService(
        HttpClient httpClient,
        IOptions<FluxGatewaySettings> settings,
        IMemoryCache cache,
        ILogger<FluxAiService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _cache = cache;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<string?> GenerateTextAsync(
        string prompt, CancellationToken cancellationToken = default)
    {
        var token = await GetOrRefreshTokenAsync(cancellationToken);

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var chatRequest = new FluxChatRequest(
            Model: _settings.Model,
            Messages:
            [
                new FluxChatMessage("system",
                    "Eres un asistente especializado en redacción de memorias de proyectos de construcción en España, " +
                    "siguiendo el Código Técnico de la Edificación (CTE) y la Ley de Ordenación de la Edificación (LOE). " +
                    "Responde siempre en español y en formato HTML limpio."),
                new FluxChatMessage("user", prompt)
            ]);

        _logger.LogInformation("Sending chat completion request to Flux Gateway for prompt of {Length} chars",
            prompt.Length);

        var response = await _httpClient.PostAsJsonAsync(
            _settings.ChatUrl, chatRequest, cancellationToken);

        response.EnsureSuccessStatusCode();

        var chatResponse = await response.Content
            .ReadFromJsonAsync<FluxChatResponse>(cancellationToken);

        var content = chatResponse?.Choices?.FirstOrDefault()?.Message?.Content;

        if (string.IsNullOrWhiteSpace(content))
        {
            _logger.LogWarning("Flux Gateway returned empty content for prompt");
            return null;
        }

        _logger.LogInformation("Received AI response of {Length} chars", content.Length);
        return content;
    }

    private async Task<string> GetOrRefreshTokenAsync(CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue(TokenCacheKey, out string? cachedToken) && !string.IsNullOrEmpty(cachedToken))
        {
            _logger.LogDebug("Using cached Flux Gateway token");
            return cachedToken;
        }

        _logger.LogInformation("Requesting new Flux Gateway authentication token");

        var authRequest = new FluxAuthRequest(_settings.ClientId, _settings.ClientSecret);

        var response = await _httpClient.PostAsJsonAsync(
            _settings.AuthUrl, authRequest, cancellationToken);

        response.EnsureSuccessStatusCode();

        var authResponse = await response.Content
            .ReadFromJsonAsync<FluxAuthResponse>(cancellationToken);

        var token = authResponse?.Token
            ?? throw new InvalidOperationException("Flux Gateway auth response did not contain a token.");

        _cache.Set(TokenCacheKey, token, TokenCacheDuration);

        _logger.LogInformation("Flux Gateway token cached for {Duration} minutes",
            TokenCacheDuration.TotalMinutes);

        return token;
    }
}
