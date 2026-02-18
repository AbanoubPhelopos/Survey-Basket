namespace Survey_Basket.Application.Contracts.Common;

public record RequestFilters
(
    int PageNumber = 1,
    int PageSize = 10,
    string? SortColumn = null,
    string? SortDirection = "ASC",
    string? SearchTerm = null
);
