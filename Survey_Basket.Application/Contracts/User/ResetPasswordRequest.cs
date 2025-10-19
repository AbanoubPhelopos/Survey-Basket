namespace Survey_Basket.Application.Contracts.User;

public record ResetPasswordRequest
(
string Email,
string Code,
string NewPassword
);
