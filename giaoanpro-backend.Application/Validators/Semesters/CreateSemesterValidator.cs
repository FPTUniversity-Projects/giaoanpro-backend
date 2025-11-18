using FluentValidation;
using giaoanpro_backend.Application.DTOs.Requests.Semesters;

namespace giaoanpro_backend.Application.Validators.Semesters
{
	public class CreateSemesterValidator : AbstractValidator<CreateSemesterRequest>
	{
		public CreateSemesterValidator()
		{
			RuleFor(x => x.Name)
				.NotEmpty().WithMessage("Name is required")
				.MaximumLength(100).WithMessage("Name must not exceed 100 characters");

			RuleFor(x => x.StartDate)
				.NotEmpty().WithMessage("Start date is required");

			RuleFor(x => x.EndDate)
				.NotEmpty().WithMessage("End date is required")
				.GreaterThan(x => x.StartDate).WithMessage("End date must be after start date");
		}
	}
}
