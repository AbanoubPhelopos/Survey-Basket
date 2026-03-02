using System.Linq.Expressions;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Survey_Basket.Application.Contracts.Votes;
using Survey_Basket.Application.Errors;
using Survey_Basket.Application.Services.VoteServices;
using Survey_Basket.Domain.Abstractions;
using Survey_Basket.Domain.Abstractions.Repositories;
using Survey_Basket.Domain.Entities;
using Xunit;

namespace Survey_Basket.Tests.Unit.Votes;

public class VoteServiceTests
{
    [Fact]
    public async Task AddAsync_ShouldReturnIdentityAlreadyExists_WhenSameEmailOrMobileVotedBefore()
    {
        var unitOfWork = new Mock<IUnitOfWork>();
        var votesRepo = new Mock<IBaseRepository<Vote>>();
        var usersRepo = new Mock<IBaseRepository<ApplicationUser>>();
        var fileStorage = new Mock<IFileAnswerStorage>();

        unitOfWork.Setup(x => x.Repository<Vote>()).Returns(votesRepo.Object);
        unitOfWork.Setup(x => x.Repository<ApplicationUser>()).Returns(usersRepo.Object);

        votesRepo
            .SetupSequence(x => x.AnyAsync(It.IsAny<Expression<Func<Vote, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false)
            .ReturnsAsync(true);

        usersRepo
            .Setup(x => x.GetByIdAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApplicationUser
            {
                Id = Guid.NewGuid(),
                NormalizedEmail = "TEST@EXAMPLE.COM",
                PhoneNumber = "+201234567890"
            });

        var httpContextAccessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                    new Claim("roles", "[\"Admin\"]")
                ], "Test"))
            }
        };

        var sut = new VoteService(unitOfWork.Object, fileStorage.Object, httpContextAccessor);

        var result = await sut.AddAsync(Guid.NewGuid(), Guid.NewGuid(), new VoteRequest([]), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(VoteErrors.VoteIdentityAlreadyExists);
    }
}
