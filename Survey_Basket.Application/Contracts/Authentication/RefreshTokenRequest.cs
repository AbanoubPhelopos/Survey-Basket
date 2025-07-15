namespace Survey_Basket.Application.Contracts.Authentication;

public record RefreshTokenRequest(
    string Token,
    string RefreshToken
    );

