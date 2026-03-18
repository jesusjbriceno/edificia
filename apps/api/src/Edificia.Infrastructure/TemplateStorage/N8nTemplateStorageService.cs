using System.Net.Http.Json;
using System.Text.Json;
using Edificia.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Edificia.Infrastructure.TemplateStorage;

public sealed class N8nTemplateStorageService : IFileStorageService
{
    private readonly HttpClient _httpClient;
    private readonly TemplateStorageSettings _settings;
    private readonly ILogger<N8nTemplateStorageService> _logger;

    public N8nTemplateStorageService(
        HttpClient httpClient,
        IOptions<TemplateStorageSettings> settings,
        ILogger<N8nTemplateStorageService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;

        if (string.IsNullOrWhiteSpace(_settings.N8nWebhookUrl))
        {
            throw new InvalidOperationException("TemplateStorage:N8nWebhookUrl es obligatorio para proveedor n8n.");
        }

        if (string.IsNullOrWhiteSpace(_settings.N8nApiSecret))
        {
            throw new InvalidOperationException("TemplateStorage:N8nApiSecret es obligatorio para proveedor n8n.");
        }
    }

    public async Task<string> SaveFileAsync(
        Stream fileStream,
        string fileName,
        string templateType,
        CancellationToken cancellationToken = default)
    {
        if (fileStream is null)
            throw new ArgumentNullException(nameof(fileStream));

        using var memory = new MemoryStream();
        if (fileStream.CanSeek)
            fileStream.Position = 0;

        await fileStream.CopyToAsync(memory, cancellationToken);
        var bytes = memory.ToArray();

        var payload = new N8nTemplateStorageEnvelope(
            ApiVersion: "1.0",
            Operation: "UPLOAD_TEMPLATE",
            OperationId: Guid.NewGuid().ToString(),
            TimestampUtc: DateTime.UtcNow,
            TenantId: "default",
            RequestedBy: "api-edificia",
            Payload: new
            {
                templateType,
                fileName,
                mimeType = "application/vnd.openxmlformats-officedocument.wordprocessingml.template",
                fileSizeBytes = bytes.LongLength,
                contentBase64 = Convert.ToBase64String(bytes)
            });

        var response = await SendAsync(payload, "upload", cancellationToken);

        if (!response.Success || string.IsNullOrWhiteSpace(response.Data?.StorageKey))
        {
            throw new InvalidOperationException(
                $"Error al guardar plantilla en n8n. Code={response.Code}, Message={response.Message}");
        }

        return response.Data.StorageKey;
    }

    public async Task<byte[]> GetFileAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        var payload = new N8nTemplateStorageEnvelope(
            ApiVersion: "1.0",
            Operation: "GET_TEMPLATE",
            OperationId: Guid.NewGuid().ToString(),
            TimestampUtc: DateTime.UtcNow,
            TenantId: "default",
            RequestedBy: "api-edificia",
            Payload: new
            {
                storageKey = relativePath
            });

        var response = await SendAsync(payload, "get", cancellationToken);

        if (!response.Success || string.IsNullOrWhiteSpace(response.Data?.ContentBase64))
        {
            throw new InvalidOperationException(
                $"Error al recuperar plantilla en n8n. Code={response.Code}, Message={response.Message}");
        }

        return Convert.FromBase64String(response.Data.ContentBase64);
    }

    public async Task<bool> DeleteFileAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        var payload = new N8nTemplateStorageEnvelope(
            ApiVersion: "1.0",
            Operation: "DELETE_TEMPLATE",
            OperationId: Guid.NewGuid().ToString(),
            TimestampUtc: DateTime.UtcNow,
            TenantId: "default",
            RequestedBy: "api-edificia",
            Payload: new
            {
                storageKey = relativePath,
                hardDelete = false
            });

        var response = await SendAsync(payload, "delete", cancellationToken);

        if (!response.Success)
        {
            if (string.Equals(response.Code, "TEMPLATE_NOT_FOUND", StringComparison.OrdinalIgnoreCase))
                return false;

            throw new InvalidOperationException(
                $"Error al eliminar plantilla en n8n. Code={response.Code}, Message={response.Message}");
        }

        return response.Data?.Deleted ?? false;
    }

    private async Task<N8nTemplateStorageResponse> SendAsync(
        N8nTemplateStorageEnvelope payload,
        string operationName,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, _settings.N8nWebhookUrl)
        {
            Content = JsonContent.Create(payload)
        };

        request.Headers.Add("X-Edificia-Auth", _settings.N8nApiSecret);
        request.Headers.Add("X-Request-Id", payload.OperationId);
        request.Headers.Add("X-Idempotency-Key", $"{operationName}-{payload.OperationId}");

        var response = await _httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError(
                "n8n template storage request failed. Status={StatusCode}, Body={Body}",
                (int)response.StatusCode,
                body);

            throw new InvalidOperationException(
                $"n8n template storage returned HTTP {(int)response.StatusCode}.");
        }

        var parsed = JsonSerializer.Deserialize<N8nTemplateStorageResponse>(body);

        return parsed ?? throw new InvalidOperationException("Respuesta n8n inválida para template storage.");
    }
}

public sealed record N8nTemplateStorageEnvelope(
    string ApiVersion,
    string Operation,
    string OperationId,
    DateTime TimestampUtc,
    string TenantId,
    string RequestedBy,
    object Payload);

public sealed record N8nTemplateStorageResponse(
    string ApiVersion,
    string Operation,
    string OperationId,
    bool Success,
    string Code,
    string? Message,
    string? Provider,
    DateTime TimestampUtc,
    N8nTemplateStorageData? Data);

public sealed record N8nTemplateStorageData(
    string? StorageKey,
    string? FileName,
    long? FileSizeBytes,
    string? Sha256,
    int? Version,
    string? ContentBase64,
    bool? Deleted);
