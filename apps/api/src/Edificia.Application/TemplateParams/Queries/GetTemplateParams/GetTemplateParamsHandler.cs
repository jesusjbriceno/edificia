using Dapper;
using Edificia.Application.Interfaces;
using Edificia.Application.TemplateParams.DTOs;
using Edificia.Application.TemplateParams.Queries;
using Edificia.Shared.Result;
using MediatR;

namespace Edificia.Application.TemplateParams.Queries.GetTemplateParams;

public sealed class GetTemplateParamsHandler
    : IRequestHandler<GetTemplateParamsQuery, Result<IReadOnlyList<TemplateParamResponse>>>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetTemplateParamsHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Result<IReadOnlyList<TemplateParamResponse>>> Handle(
        GetTemplateParamsQuery request,
        CancellationToken cancellationToken)
    {
        var whereClause = string.Empty;
        var parameters = new DynamicParameters();

        if (request.IsActive.HasValue)
        {
            whereClause = "WHERE is_active = @IsActive";
            parameters.Add("IsActive", request.IsActive.Value);
        }

        using var connection = _connectionFactory.CreateConnection();
        var items = await connection.QueryAsync<TemplateParamResponse>(
            TemplateParamSqlQueries.GetAll(whereClause),
            parameters);

        return Result.Success((IReadOnlyList<TemplateParamResponse>)items.ToList().AsReadOnly());
    }
}
