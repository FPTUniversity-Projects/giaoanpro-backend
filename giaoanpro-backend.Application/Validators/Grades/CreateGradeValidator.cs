using FluentValidation;
using giaoanpro_backend.Application.DTOs.Requests.Grades;

namespace giaoanpro_backend.Application.Validators.Grades
{
	public class CreateGradeValidator : AbstractValidator<CreateGradeRequest>
	{
		public CreateGradeValidator()
		{
			RuleFor(x => x.Level)
				.GreaterThan(0).WithMessage("Level must be greater than 0")
				.LessThanOrEqualTo(12).WithMessage("Level must be less than or equal to 12");
		}
	}
}
