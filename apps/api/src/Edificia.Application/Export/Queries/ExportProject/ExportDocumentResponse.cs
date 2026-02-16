namespace Edificia.Application.Export.Queries.ExportProject;

/// <summary>
/// Response containing the exported document file data.
/// </summary>
/// <param name="FileContent">The .docx file as a byte array.</param>
/// <param name="FileName">Sanitized filename for the download.</param>
/// <param name="ContentType">MIME type of the document.</param>
public sealed record ExportDocumentResponse(
    byte[] FileContent,
    string FileName,
    string ContentType);
