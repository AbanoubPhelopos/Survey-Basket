﻿namespace Survey_Basket.Application.Contracts.Authentication;

public record ConfirmEmailRequest(
    string UserId,
    string Code
    );