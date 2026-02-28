using System.Text;
using Dapper;
using Edificia.Application.Interfaces;
using Edificia.Application.Templates.DTOs;
using Edificia.Shared.Result;
using MediatR;

namespace Edificia.Application.Templates.Queries.GetTemplates;

public sealed class GetTemplatesHandler
    : IRequestHandler<GetTemplatesQuery, Result<IReadOnlyList<TemplateResponse>>>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetTemplatesHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Result<IReadOnlyList<TemplateResponse>>> Handle(
        GetTemplatesQuery request,
        CancellationToken cancellationToken)
    {
        var whereClause = new StringBuilder();
        var parameters = new DynamicParameters();
        var conditions = new List<string>();

        if (!string.IsNullOrWhiteSpace(request.TemplateType))
        {
            conditions.Add("template_type = @TemplateType");
            parameters.Add("TemplateType", request.TemplateType);
        }

        if (request.IsActive.HasValue)
        {
            conditions.Add("is_active = @IsActive");
            parameters.Add("IsActive", request.IsActive.Value);
        }

        if (conditions.Count > 0)
        {
            whereClause.Append(" WHERE ");
            whereClause.Append(string.Join(" AND ", conditions));
        }

        using var connection = _connectionFactory.CreateConnection();

        var items = await connection.QueryAsync<TemplateResponse>(
            TemplateSqlQueries.GetAll(whereClause.ToString()),
            parameters);

        return Result.Success((IReadOnlyList<TemplateResponse>)items.ToList().AsReadOnly());
    }
}
