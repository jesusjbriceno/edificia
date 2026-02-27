using Edificia.Shared.Result;

namespace Edificia.Application.Interfaces;

/// <summary>
/// Validates uploaded template binaries before persisting them.
/// </summary>
public interface ITemplateFormatValidator
{
    Result Validate(string templateType, byte[] fileContent);
}
