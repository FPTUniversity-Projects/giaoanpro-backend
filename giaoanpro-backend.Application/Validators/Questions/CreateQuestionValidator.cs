using FluentValidation;
using giaoanpro_backend.Application.DTOs.Requests.Questions;
using giaoanpro_backend.Domain.Enums;

namespace giaoanpro_backend.Application.Validators.Questions
{
	public class CreateQuestionValidator : AbstractValidator<CreateQuestionRequest>
	{
		public CreateQuestionValidator()
		{
			// No validation rules for create; allow service layer to handle any checks
		}
	}
}
