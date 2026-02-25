using Survey_Basket.Application.Abstractions;

namespace Survey_Basket.Application.Contracts.Common;

public sealed record ServiceListResult<TItem, TStats>(
    PagedList<TItem> Items,
    TStats Stats
);
