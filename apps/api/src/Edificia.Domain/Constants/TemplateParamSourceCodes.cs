namespace Edificia.Domain.Constants;

/// <summary>
/// Canonical source codes used to resolve global template parameters.
/// </summary>
public static class TemplateParamSourceCodes
{
    public const string ProjectTitle = "PROJECT_TITLE";
    public const string ProjectDescription = "PROJECT_DESCRIPTION";
    public const string ProjectAddress = "PROJECT_ADDRESS";
    public const string InterventionType = "INTERVENTION_TYPE";
    public const string IsLoeRequired = "IS_LOE_REQUIRED";
    public const string CadastralReference = "CADASTRAL_REFERENCE";
    public const string LocalRegulations = "LOCAL_REGULATIONS";
    public const string ExportDate = "EXPORT_DATE";
    public const string ExportDateTime = "EXPORT_DATETIME";

    public static readonly string[] All =
    [
        ProjectTitle,
        ProjectDescription,
        ProjectAddress,
        InterventionType,
        IsLoeRequired,
        CadastralReference,
        LocalRegulations,
        ExportDate,
        ExportDateTime
    ];
}
