# **🛠️ Guía de Implementación: Migración a n8n**

Esta guía detalla los pasos técnicos para sustituir la integración directa de IA (Flux Gateway) por la delegación en n8n.

> **Versión:** 2.0 — Actualizada 17/02/2026 (alineada con código real)

## **1. Cambios en Configuración**

### **1.1. Backend (appsettings.json)**

Eliminamos el bloque `FluxGateway` y lo sustituimos por `AI`:

```json
{
  "AI": {
    "Provider": "n8n",
    "WebhookUrl": "https://n8n.tu-dominio.com/webhook/generar-memoria",
    "ApiSecret": "ESTE_SECRETO_DEBE_COINCIDIR_EN_N8N_HEADER_AUTH",
    "TimeoutSeconds": 120
  }
}
```

### **1.2. Docker (docker-compose.prod.yml / docker-compose.apps.yml)**

Se eliminan las variables `FluxGateway__*` y se añaden las de `AI__*`:

```yaml
api:
  environment:
    # ELIMINAR:
    # - FluxGateway__ClientId=...
    # - FluxGateway__ClientSecret=...

    # AÑADIR:
    - AI__WebhookUrl=${N8N_WEBHOOK_URL}
    - AI__ApiSecret=${N8N_API_SECRET}
    - AI__TimeoutSeconds=${N8N_TIMEOUT:-120}
```

### **1.3. .env.example**

```dotenv
# ── IA (n8n Webhook) ──
N8N_WEBHOOK_URL=https://n8n.tu-dominio.com/webhook/generar-memoria
N8N_API_SECRET=CHANGE_ME_shared_secret
N8N_TIMEOUT=120
```

## **2. Refactorización Backend (.NET)**

### **2.1. Interfaz IAiService (Application)**

Se modifica la firma para recibir un request tipado en lugar de un string.
Se mantiene `CancellationToken` como en el contrato actual.

```csharp
public interface IAiService
{
    Task<string?> GenerateTextAsync(
        AiGenerationRequest request,
        CancellationToken cancellationToken = default);
}
```

### **2.2. Settings — N8nAiSettings (Infrastructure)**

Sigue el patrón `IOptions<T>` del proyecto (como `JwtSettings`, `EmailSettings`):

```csharp
public sealed class N8nAiSettings
{
    public const string SectionName = "AI";
    public string Provider { get; set; } = "n8n";
    public string WebhookUrl { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 120;
}
```

### **2.3. Implementación N8nAiService (Infrastructure)**

Sustituye a `FluxAiService`. Usa `IOptions<N8nAiSettings>` (NO `IConfiguration` directamente).

```csharp
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

    public async Task<string?> GenerateTextAsync(
        AiGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, _settings.WebhookUrl);
        httpRequest.Headers.Add("X-Edificia-Auth", _settings.ApiSecret);
        httpRequest.Content = JsonContent.Create(request);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<N8nAiResponse>(cancellationToken);

        return result?.GeneratedText;
    }
}
```

### **2.4. Registro DI (Infrastructure/DependencyInjection.cs)**

```csharp
// ---------- AI Service (n8n Webhook) ----------
services.Configure<N8nAiSettings>(
    configuration.GetSection(N8nAiSettings.SectionName));

var aiSettings = configuration
    .GetSection(N8nAiSettings.SectionName)
    .Get<N8nAiSettings>() ?? new N8nAiSettings();

services.AddHttpClient<IAiService, N8nAiService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(aiSettings.TimeoutSeconds);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});
```

## **3. DTOs de Intercambio**

Definidos en `Edificia.Application` (junto a `IAiService`) para respetar la Dependency Rule.

```csharp
/// <summary>
/// Request DTO sent to the n8n webhook for AI text generation.
/// </summary>
public sealed record AiGenerationRequest(
    string SectionCode,
    string ProjectType,
    TechnicalContext? TechnicalContext,
    string? UserInstructions);

/// <summary>
/// Typed technical context sent to n8n.
/// </summary>
public sealed record TechnicalContext(
    string? ProjectTitle,
    string? InterventionType,
    bool? IsLoeRequired,
    string? Address,
    string? LocalRegulations,
    string? ExistingContent);

/// <summary>
/// Response DTO received from the n8n webhook.
/// </summary>
public sealed record N8nAiResponse(
    string? GeneratedText,
    AiUsageMetadata? Usage);

/// <summary>
/// Optional metadata about the AI generation (model used, tokens, etc.).
/// </summary>
public sealed record AiUsageMetadata(
    string? Model,
    int? Tokens);
```

## **4. Código Eliminado**

| Archivo | Motivo |
|---------|--------|
| `FluxAiService.cs` | Reemplazado por `N8nAiService` |
| `FluxGatewayDtos.cs` | DTOs de Flux ya no necesarios |
| `FluxGatewaySettings.cs` | Reemplazado por `N8nAiSettings` |
| `PromptTemplateService.cs` | La construcción del prompt se delega a n8n |
| `IPromptTemplateService.cs` | Interfaz del servicio eliminado |
| `SectionPromptContext.cs` | Record usado solo por PromptTemplateService |

## **5. Handler Actualizado**

`GenerateSectionTextHandler` ya no usa `IPromptTemplateService`. En su lugar:
1. Carga el proyecto del repositorio.
2. Construye un `AiGenerationRequest` con los datos del proyecto.
3. Envía el request a `IAiService.GenerateTextAsync()`.
4. Retorna `GeneratedTextResponse` con el texto generado.
