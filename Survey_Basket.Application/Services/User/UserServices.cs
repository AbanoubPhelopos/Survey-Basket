using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Survey_Basket.Application.Abstraction;
using Survey_Basket.Application.Contracts.User;
using Survey_Basket.Application.Errors;
using Survey_Basket.Domain.Models;

namespace Survey_Basket.Application.Services.User;

public class UserServices(UserManager<ApplicationUser> userManager) : IUserServices
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;


    public async Task<Result<UserProfileResponse>> GetUserProfile(Guid userId)
    {
        var user = await _userManager.Users.Where(u => u.Id == userId)
            .ProjectToType<UserProfileResponse>()
            .SingleAsync();

        return Result.Success(user);
    } 
    public async Task<Result> UpdateUserProfile(Guid userId, UpdateProfileRequest request,CancellationToken cancellationToken)
    {
        /*var user = await _userManager.FindByIdAsync(userId.ToString());

        user = request.Adapt(user);

        var result = await _userManager.UpdateAsync(user!);*/

        await _userManager.Users
            .Where(u => u.Id == userId)
            .ExecuteUpdateAsync(u => u
                .SetProperty(u => u.FirstName, request.FirstName)
                .SetProperty(u => u.LastName, request.LastName), cancellationToken);

        return Result.Success();
    }

    public async Task<Result> ChangePassword(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        var result = await _userManager.ChangePasswordAsync(user!, request.CurrentPassword, request.NewPassword);

        if (result.Succeeded)
            return Result.Success();
        
        var error = result.Errors.First();
            return Result.Failure(new Error(error.Code, error.Description,StatusCodes.Status400BadRequest));

    }
}
