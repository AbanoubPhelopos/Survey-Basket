using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Survey_Basket.Application.Abstractions;
using Survey_Basket.Application.Contracts.Polls;
using Survey_Basket.Domain.Entities;
using Survey_Basket.Tests.Abstractions;
using Xunit;

namespace Survey_Basket.Tests.Integration.Polls;

public class PollsTests : BaseIntegrationTest
{
    public PollsTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetPolls_ShouldReturnPaginatedList_WhenPollsExist()
    {
        // Arrange
        var polls = new List<Poll>
        {
            new() { Title = "Poll 1", Summary = "Summary 1", IsPublished = true },
            new() { Title = "Poll 2", Summary = "Summary 2", IsPublished = true },
            new() { Title = "Poll 3", Summary = "Summary 3", IsPublished = true }
        };

        await DbContext.Set<Poll>().AddRangeAsync(polls);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await HttpClient.GetAsync("api/polls?pageNumber=1&pageSize=2");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<PagedList<PollResponse>>();
        
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(3);
        result.TotalPages.Should().Be(2);
        result.PageNumber.Should().Be(1);
    }
}
