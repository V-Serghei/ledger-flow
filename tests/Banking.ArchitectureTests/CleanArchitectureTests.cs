using Banking.Api.Controllers;
using Banking.Application.Accounts;
using Banking.Domain.Accounts;
using Banking.Infrastructure.Persistence;
using FluentAssertions;
using NetArchTest.Rules;

namespace Banking.ArchitectureTests;

public sealed class CleanArchitectureTests
{
    [Fact]
    public void Domain_should_not_reference_application_infrastructure_or_api()
    {
        var result = Types.InAssembly(typeof(Account).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny("Banking.Application", "Banking.Infrastructure", "Banking.Api")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Application_should_not_reference_infrastructure_or_api()
    {
        var result = Types.InAssembly(typeof(IAccountService).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny("Banking.Infrastructure", "Banking.Api")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Infrastructure_should_not_be_referenced_by_domain()
    {
        var result = Types.InAssembly(typeof(Account).Assembly)
            .ShouldNot()
            .HaveDependencyOn("Banking.Infrastructure")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Controllers_should_not_access_db_context_directly()
    {
        var result = Types.InAssembly(typeof(AuthController).Assembly)
            .That()
            .HaveNameEndingWith("Controller")
            .ShouldNot()
            .HaveDependencyOn(typeof(BankingDbContext).FullName)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }
}

