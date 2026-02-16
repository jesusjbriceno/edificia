using Edificia.Application.Auth.Commands.Login;
using Edificia.Application.Auth.DTOs;
using FluentAssertions;

namespace Edificia.Application.Tests.Auth;

public class LoginMappingTests
{
    [Fact]
    public void ExplicitOperator_ShouldMapAllFields()
    {
        var request = new LoginRequest("admin@edificia.dev", "SecurePass123!");

        var command = (LoginCommand)request;

        command.Email.Should().Be("admin@edificia.dev");
        command.Password.Should().Be("SecurePass123!");
    }
}
