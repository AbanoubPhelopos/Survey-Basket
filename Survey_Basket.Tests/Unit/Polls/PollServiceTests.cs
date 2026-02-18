using System.Linq.Expressions;
using FluentAssertions;
using Moq;
using Survey_Basket.Application.Contracts.Common;
using Survey_Basket.Application.Contracts.Polls;
using Survey_Basket.Application.Errors;
using Survey_Basket.Application.Services.NotificationServices;
using Survey_Basket.Application.Services.PollServices;
using Survey_Basket.Domain.Abstractions;
using Survey_Basket.Domain.Abstractions.Repositories;
using Survey_Basket.Domain.Entities;
using Xunit;

namespace Survey_Basket.Tests.Unit.Polls;

public class PollServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IBaseRepository<Poll>> _pollRepoMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly PollService _sut;

    public PollServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _pollRepoMock = new Mock<IBaseRepository<Poll>>();
        _notificationServiceMock = new Mock<INotificationService>();

        _unitOfWorkMock.Setup(u => u.Repository<Poll>()).Returns(_pollRepoMock.Object);

        _sut = new PollService(_unitOfWorkMock.Object, _notificationServiceMock.Object);
    }

    #region Get(Guid id)

    [Fact]
    public async Task Get_ById_ShouldReturnPollResponse_WhenPollExists()
    {
        // Arrange
        var pollId = Guid.NewGuid();
        var poll = new Poll
        {
            Id = pollId,
            Title = "Test Poll",
            Summary = "Test Summary",
            IsPublished = true,
            StartedAt = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        _pollRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(poll);

        // Act
        var result = await _sut.Get(pollId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(pollId);
        result.Value.Title.Should().Be("Test Poll");
        result.Value.Summary.Should().Be("Test Summary");
    }

    [Fact]
    public async Task Get_ById_ShouldReturnFailure_WhenPollNotFound()
    {
        // Arrange
        _pollRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Poll?)null);

        // Act
        var result = await _sut.Get(Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(PollErrors.PollNotFound);
    }

    #endregion

    #region Get(RequestFilters)

    [Fact]
    public async Task Get_WithFilters_ShouldReturnPagedList()
    {
        // Arrange
        var filters = new RequestFilters(PageNumber: 1, PageSize: 2);
        var polls = new List<Poll>
        {
            new() { Id = Guid.NewGuid(), Title = "Poll 1", Summary = "Summary 1" },
            new() { Id = Guid.NewGuid(), Title = "Poll 2", Summary = "Summary 2" }
        };

        _pollRepoMock
            .Setup(r => r.GetPagedAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<Expression<Func<Poll, bool>>?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((polls.AsEnumerable(), 5));

        // Act
        var result = await _sut.Get(filters);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
        result.Value.TotalCount.Should().Be(5);
        result.Value.PageNumber.Should().Be(1);
        result.Value.TotalPages.Should().Be(3); // ceil(5/2) = 3
    }

    #endregion

    #region CreatePollAsync

    [Fact]
    public async Task CreatePollAsync_ShouldReturnSuccess_WhenPollDoesNotExist()
    {
        // Arrange
        var request = new CreatePollRequests("New Poll", "New Description", DateOnly.MinValue, null);

        _pollRepoMock
            .Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Poll, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _pollRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Poll>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Poll());

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _sut.CreatePollAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _pollRepoMock.Verify(
            r => r.AddAsync(It.IsAny<Poll>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreatePollAsync_ShouldReturnFailure_WhenPollAlreadyExists()
    {
        // Arrange
        var request = new CreatePollRequests("Existing Poll", "Description", DateOnly.MinValue, null);

        _pollRepoMock
            .Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Poll, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.CreatePollAsync(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(PollErrors.PollAlreadyExists);

        _pollRepoMock.Verify(
            r => r.AddAsync(It.IsAny<Poll>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region DeletePoll

    [Fact]
    public async Task DeletePoll_ShouldReturnSuccess_WhenPollExists()
    {
        // Arrange
        var pollId = Guid.NewGuid();
        var poll = new Poll { Id = pollId, Title = "To Delete" };

        _pollRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(poll);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _sut.DeletePoll(pollId);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _pollRepoMock.Verify(r => r.Remove(poll), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeletePoll_ShouldReturnFailure_WhenPollNotFound()
    {
        // Arrange
        _pollRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Poll?)null);

        // Act
        var result = await _sut.DeletePoll(Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(PollErrors.PollNotFound);

        _pollRepoMock.Verify(r => r.Remove(It.IsAny<Poll>()), Times.Never);
    }

    #endregion

    #region UpdatePoll

    [Fact]
    public async Task UpdatePoll_ShouldReturnSuccess_WhenValidUpdate()
    {
        // Arrange
        var pollId = Guid.NewGuid();
        var existingPoll = new Poll { Id = pollId, Title = "Old Title", Summary = "Old Summary" };
        var updateRequest = new UpdatePollRequests("New Title", "New Description", DateOnly.MinValue, null);

        _pollRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPoll);

        // No duplicate title
        _pollRepoMock
            .Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Poll, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _sut.UpdatePoll(pollId, updateRequest);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _pollRepoMock.Verify(r => r.Update(existingPoll), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdatePoll_ShouldReturnFailure_WhenPollNotFound()
    {
        // Arrange
        _pollRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Poll?)null);

        // Act
        var result = await _sut.UpdatePoll(Guid.NewGuid(), new UpdatePollRequests("Title", "Desc", DateOnly.MinValue, null));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(PollErrors.PollNotFound);
    }

    [Fact]
    public async Task UpdatePoll_ShouldReturnFailure_WhenDuplicateTitleExists()
    {
        // Arrange
        var pollId = Guid.NewGuid();
        var existingPoll = new Poll { Id = pollId, Title = "Old Title" };

        _pollRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPoll);

        // Duplicate title exists
        _pollRepoMock
            .Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Poll, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.UpdatePoll(pollId, new UpdatePollRequests("Duplicate Title", "Desc", DateOnly.MinValue, null));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(PollErrors.PollAlreadyExists);

        _pollRepoMock.Verify(r => r.Update(It.IsAny<Poll>()), Times.Never);
    }

    #endregion

    #region TogglePublishStatusAsync

    [Fact]
    public async Task TogglePublishStatusAsync_ShouldToggleStatus_WhenPollExists()
    {
        // Arrange
        var pollId = 1;
        var poll = new Poll
        {
            Id = Guid.NewGuid(),
            Title = "Toggle Poll",
            IsPublished = false,
            StartedAt = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)) // future date to skip Hangfire
        };

        _pollRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(poll);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _sut.TogglePublishStatusAsync(pollId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        poll.IsPublished.Should().BeTrue(); // toggled from false to true

        _pollRepoMock.Verify(r => r.Update(poll), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TogglePublishStatusAsync_ShouldReturnFailure_WhenPollNotFound()
    {
        // Arrange
        _pollRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Poll?)null);

        // Act
        var result = await _sut.TogglePublishStatusAsync(999);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(PollErrors.PollNotFound);
    }

    #endregion

    #region GetCurrent

    [Fact]
    public async Task GetCurrent_ShouldReturnPublishedActivePolls()
    {
        // Arrange
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var polls = new List<Poll>
        {
            new()
            {
                Id = Guid.NewGuid(), Title = "Active Poll", Summary = "Active",
                IsPublished = true, StartedAt = today.AddDays(-1), EndedAt = today.AddDays(5)
            }
        };

        _pollRepoMock
            .Setup(r => r.GetAllAsync(
                It.IsAny<Expression<Func<Poll, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(polls);

        // Act
        var result = await _sut.GetCurrent();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }

    #endregion
}
