namespace Survey_Basket.Application.Contracts.User;

public sealed record CreateCompanyUserRecordRequest(
    string DisplayName,
    string BusinessIdentifier
);
