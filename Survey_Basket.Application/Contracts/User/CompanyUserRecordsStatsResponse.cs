namespace Survey_Basket.Application.Contracts.User;

public sealed record CompanyUserRecordsStatsResponse(
    int TotalRecords,
    int ShortIdentifiers,
    int LongIdentifiers
);
