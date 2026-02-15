using Edificia.Application.Common;
using FluentAssertions;

namespace Edificia.Application.Tests.Common;

public class PagedResponseTests
{
    [Fact]
    public void TotalPages_ShouldCalculateCorrectly()
    {
        var response = new PagedResponse<string>(
            Items: ["a", "b"],
            TotalCount: 25,
            Page: 1,
            PageSize: 10);

        response.TotalPages.Should().Be(3);
    }

    [Fact]
    public void TotalPages_ShouldBeOne_WhenCountEqualsPageSize()
    {
        var response = new PagedResponse<string>(
            Items: ["a"],
            TotalCount: 10,
            Page: 1,
            PageSize: 10);

        response.TotalPages.Should().Be(1);
    }

    [Fact]
    public void TotalPages_ShouldBeZero_WhenEmpty()
    {
        var response = new PagedResponse<string>(
            Items: [],
            TotalCount: 0,
            Page: 1,
            PageSize: 10);

        response.TotalPages.Should().Be(0);
    }

    [Fact]
    public void HasNextPage_ShouldBeTrue_WhenNotOnLastPage()
    {
        var response = new PagedResponse<string>(
            Items: ["a"],
            TotalCount: 25,
            Page: 1,
            PageSize: 10);

        response.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public void HasNextPage_ShouldBeFalse_WhenOnLastPage()
    {
        var response = new PagedResponse<string>(
            Items: ["a"],
            TotalCount: 25,
            Page: 3,
            PageSize: 10);

        response.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public void HasPreviousPage_ShouldBeFalse_WhenOnFirstPage()
    {
        var response = new PagedResponse<string>(
            Items: ["a"],
            TotalCount: 25,
            Page: 1,
            PageSize: 10);

        response.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public void HasPreviousPage_ShouldBeTrue_WhenNotOnFirstPage()
    {
        var response = new PagedResponse<string>(
            Items: ["a"],
            TotalCount: 25,
            Page: 2,
            PageSize: 10);

        response.HasPreviousPage.Should().BeTrue();
    }
}
