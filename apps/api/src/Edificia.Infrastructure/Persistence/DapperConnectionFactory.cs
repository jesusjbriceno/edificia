using System.Data;
using Edificia.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Edificia.Infrastructure.Persistence;

/// <summary>
/// Creates NpgsqlConnection instances for Dapper queries (read-side of CQRS).
/// </summary>
public sealed class DapperConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public DapperConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' not found in configuration.");
    }

    public IDbConnection CreateConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }
}
