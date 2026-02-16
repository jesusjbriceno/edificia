namespace Edificia.Domain.Enums;

/// <summary>
/// Estado del proyecto dentro del flujo de trabajo.
/// </summary>
public enum ProjectStatus
{
    /// <summary>Borrador inicial — en proceso de configuración.</summary>
    Draft = 0,

    /// <summary>En redacción — el contenido de la memoria se está elaborando.</summary>
    InProgress = 1,

    /// <summary>Completado — memoria finalizada y exportable.</summary>
    Completed = 2,

    /// <summary>Archivado — proyecto cerrado/entregado.</summary>
    Archived = 3
}
