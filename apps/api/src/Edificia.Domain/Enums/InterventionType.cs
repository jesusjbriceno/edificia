namespace Edificia.Domain.Enums;

/// <summary>
/// Tipo de intervención del proyecto de construcción.
/// Determina qué capítulos normativos (CTE/LOE) se aplican.
/// </summary>
public enum InterventionType
{
    /// <summary>Obra Nueva — aplica normativa CTE completa.</summary>
    NewConstruction = 0,

    /// <summary>Reforma — exime ciertos capítulos estructurales.</summary>
    Reform = 1,

    /// <summary>Ampliación — normativa parcial según alcance.</summary>
    Extension = 2
}
