using giaoanpro_backend.Application.DTOs.Responses.Questions;
using giaoanpro_backend.Domain.Entities;
using Mapster;

namespace giaoanpro_backend.Application.Mappings
{
	public class QuestionRegister : IRegister
	{
		public void Register(TypeAdapterConfig config)
		{
			config.NewConfig<Question, GetQuestionResponse>()
				.Map(dest => dest.Id, src => src.Id)
				.Map(dest => dest.Text, src => src.Text)
				.Map(dest => dest.QuestionType, src => src.QuestionType.ToString())
				.Map(dest => dest.DifficultyLevel, src => src.DifficultyLevel.ToString())
				.Map(dest => dest.AwarenessLevel, src => src.AwarenessLevel.ToString())
				.Map(dest => dest.CreatedAt, src => src.CreatedAt)
				.Map(dest => dest.UpdatedAt, src => src.UpdatedAt)
				.Map(dest => dest.Options, src => src.Options == null ? new List<QuestionOption>() : src.Options)
				.AfterMapping((src, dest) =>
				{
					// Ensure Options list is never null
					dest.Options ??= new List<QuestionOptionDto>();
				});

			config.NewConfig<QuestionOption, QuestionOptionDto>()
				.Map(dest => dest.Id, src => src.Id)
				.Map(dest => dest.Text, src => src.Text)
				.Map(dest => dest.IsCorrect, src => src.IsCorrect);
		}
	}
}
