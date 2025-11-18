using FluentValidation;
using giaoanpro_backend.Application.DTOs.Requests.Questions;
using giaoanpro_backend.Domain.Enums;

namespace giaoanpro_backend.Application.Validators.Questions
{
	public class UpdateQuestionValidator : AbstractValidator<UpdateQuestionRequest>
	{
		public UpdateQuestionValidator()
		{

		}
	}
}
