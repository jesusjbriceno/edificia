using Edificia.Application.Interfaces;
using Edificia.Application.Users.DTOs;
using Edificia.Application.Users.Queries.GetUserById;
using Edificia.Shared.Result;
using FluentAssertions;
using Moq;
using System.Data;

namespace Edificia.Application.Tests.Users;

public class GetUserByIdHandlerTests
{
    private readonly Mock<IDbConnectionFactory> _connectionFactoryMock;
    private readonly GetUserByIdHandler _handler;

    public GetUserByIdHandlerTests()
    {
        _connectionFactoryMock = new Mock<IDbConnectionFactory>();
        _handler = new GetUserByIdHandler(_connectionFactoryMock.Object);
    }

    [Fact]
    public void Handler_ShouldImplementIRequestHandler()
    {
        _handler.Should().BeAssignableTo<MediatR.IRequestHandler<GetUserByIdQuery, Result<UserResponse>>>();
    }

    [Fact]
    public void Query_ShouldContainId()
    {
        var id = Guid.NewGuid();
        var query = new GetUserByIdQuery(id);

        query.Id.Should().Be(id);
    }
}
