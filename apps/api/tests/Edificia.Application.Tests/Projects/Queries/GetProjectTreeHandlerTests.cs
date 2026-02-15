using Edificia.Application.Interfaces;
using Edificia.Application.Projects.Queries.GetProjectTree;
using Edificia.Shared.Result;
using FluentAssertions;
using Moq;
using System.Data;

namespace Edificia.Application.Tests.Projects.Queries;

public class GetProjectTreeHandlerTests
{
    private readonly Mock<IDbConnectionFactory> _connectionFactoryMock;
    private readonly GetProjectTreeHandler _handler;

    public GetProjectTreeHandlerTests()
    {
        _connectionFactoryMock = new Mock<IDbConnectionFactory>();
        _handler = new GetProjectTreeHandler(_connectionFactoryMock.Object);
    }

    [Fact]
    public void Handler_ShouldImplementIRequestHandler()
    {
        _handler.Should().BeAssignableTo<MediatR.IRequestHandler<GetProjectTreeQuery, Result<ContentTreeResponse>>>();
    }

    [Fact]
    public void Query_ShouldContainProjectId()
    {
        var projectId = Guid.NewGuid();
        var query = new GetProjectTreeQuery(projectId);

        query.ProjectId.Should().Be(projectId);
    }
}
