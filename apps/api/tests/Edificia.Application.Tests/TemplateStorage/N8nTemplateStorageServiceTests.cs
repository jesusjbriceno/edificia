using System.Net;
using System.Text;
using System.Text.Json;
using Edificia.Infrastructure.TemplateStorage;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Edificia.Application.Tests.TemplateStorage;

public class N8nTemplateStorageServiceTests
{
    [Fact]
    public async Task SaveFileAsync_ShouldReturnStorageKey_WhenN8nRespondsSuccess()
    {
        var handler = new FakeHttpMessageHandler(_ =>
        {
            var response = new N8nTemplateStorageResponse(
                ApiVersion: "1.0",
                Operation: "UPLOAD_TEMPLATE",
                OperationId: Guid.NewGuid().ToString(),
                Success: true,
                Code: "TEMPLATE_STORAGE_OK",
                Message: "ok",
                Provider: "s3",
                TimestampUtc: DateTime.UtcNow,
                Data: new N8nTemplateStorageData(
                    StorageKey: "templates/memoria/v1.dotx",
                    FileName: "plantilla.dotx",
                    FileSizeBytes: 100,
                    Sha256: null,
                    Version: 1,
                    ContentBase64: null,
                    Deleted: null));

            var content = JsonSerializer.Serialize(response);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(content, Encoding.UTF8, "application/json")
            };
        });

        var httpClient = new HttpClient(handler);
        var settings = Options.Create(new TemplateStorageSettings
        {
            Provider = "n8n",
            N8nWebhookUrl = "http://localhost:5678/webhook/template-storage",
            N8nApiSecret = "secret",
            TimeoutSeconds = 30
        });

        var service = new N8nTemplateStorageService(httpClient, settings, Mock.Of<ILogger<N8nTemplateStorageService>>());

        await using var stream = new MemoryStream(new byte[] { 10, 11, 12 });
        var storageKey = await service.SaveFileAsync(stream, "plantilla.dotx", "MemoriaTecnica");

        storageKey.Should().Be("templates/memoria/v1.dotx");
    }

    [Fact]
    public async Task GetFileAsync_ShouldDecodeBase64Content_WhenN8nRespondsSuccess()
    {
        var expectedBytes = new byte[] { 10, 20, 30, 40 };
        var base64 = Convert.ToBase64String(expectedBytes);

        var handler = new FakeHttpMessageHandler(_ =>
        {
            var response = new N8nTemplateStorageResponse(
                ApiVersion: "1.0",
                Operation: "GET_TEMPLATE",
                OperationId: Guid.NewGuid().ToString(),
                Success: true,
                Code: "TEMPLATE_STORAGE_OK",
                Message: "ok",
                Provider: "s3",
                TimestampUtc: DateTime.UtcNow,
                Data: new N8nTemplateStorageData(
                    StorageKey: "templates/memoria/v1.dotx",
                    FileName: null,
                    FileSizeBytes: expectedBytes.Length,
                    Sha256: null,
                    Version: null,
                    ContentBase64: base64,
                    Deleted: null));

            var content = JsonSerializer.Serialize(response);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(content, Encoding.UTF8, "application/json")
            };
        });

        var httpClient = new HttpClient(handler);
        var settings = Options.Create(new TemplateStorageSettings
        {
            Provider = "n8n",
            N8nWebhookUrl = "http://localhost:5678/webhook/template-storage",
            N8nApiSecret = "secret",
            TimeoutSeconds = 30
        });

        var service = new N8nTemplateStorageService(httpClient, settings, Mock.Of<ILogger<N8nTemplateStorageService>>());

        var result = await service.GetFileAsync("templates/memoria/v1.dotx");

        result.Should().Equal(expectedBytes);
    }

    private sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responseFactory;

        public FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
        {
            _responseFactory = responseFactory;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(_responseFactory(request));
        }
    }
}
