
namespace Survey_Basket.Application.Contracts.User;

public record ChangePasswordRequest
(
    string CurrentPassword,
    string NewPassword
);
