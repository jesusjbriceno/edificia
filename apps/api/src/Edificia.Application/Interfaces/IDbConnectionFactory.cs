using System.Data;

namespace Edificia.Application.Interfaces;

/// <summary>
/// Factory for creating database connections used by Dapper (read-side / Queries).
/// Implemented in Infrastructure layer.
/// </summary>
public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}
