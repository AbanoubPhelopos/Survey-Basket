using Mapster;
using Microsoft.AspNetCore.Identity.Data;
using Survey_Basket.Application.Contracts.Question;
using Survey_Basket.Domain.Models;

namespace Survey_Basket.Application.Mapping;

public class MappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<QuestionRequest, Question>()
            .Map(dest => dest.Answers, src => src.Answers.Select(answer => new Answer { Content = answer }));

        config.NewConfig<RegisterRequest, ApplicationUser>()
            .Map(dest => dest.UserName, src => src.Email);
    }
}
