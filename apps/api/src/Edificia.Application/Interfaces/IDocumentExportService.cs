namespace Edificia.Application.Interfaces;

/// <summary>
/// Service responsible for generating document exports from project content.
/// </summary>
public interface IDocumentExportService
{
    /// <summary>
    /// Exports a project's content tree to a .docx byte array.
    /// </summary>
    /// <param name="data">Structured data containing project info and content tree JSON.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The generated .docx file as a byte array.</returns>
    Task<byte[]> ExportToDocxAsync(ExportDocumentData data, CancellationToken cancellationToken = default);

    /// <summary>
    /// Exports a project's content tree to a .docx byte array using a .dotx template.
    /// </summary>
    /// <param name="data">Structured data containing project info and content tree JSON.</param>
    /// <param name="templateContent">The .dotx template content as byte array.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The generated .docx file as a byte array.</returns>
    Task<byte[]> ExportToDocxWithTemplateAsync(
        ExportDocumentData data,
        byte[] templateContent,
        CancellationToken cancellationToken = default);
}
