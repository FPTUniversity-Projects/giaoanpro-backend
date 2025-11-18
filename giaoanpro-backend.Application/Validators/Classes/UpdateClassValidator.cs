using FluentValidation;
using giaoanpro_backend.Application.DTOs.Requests.Classes;

namespace giaoanpro_backend.Application.Validators.Classes
{
	public class UpdateClassValidator : AbstractValidator<UpdateClassRequest>
	{
		public UpdateClassValidator()
		{
			RuleFor(x => x.TeacherId)
				.NotEmpty().WithMessage("Teacher ID is required");

			RuleFor(x => x.GradeId)
				.NotEmpty().WithMessage("Grade ID is required");

			RuleFor(x => x.SemesterId)
				.NotEmpty().WithMessage("Semester ID is required");

			RuleFor(x => x.Name)
				.NotEmpty().WithMessage("Name is required")
				.MaximumLength(100).WithMessage("Name must not exceed 100 characters");
		}
	}
}
