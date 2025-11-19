using FluentValidation;
using giaoanpro_backend.Application.DTOs.Requests.Subjects;

namespace giaoanpro_backend.Application.Validators.Subjects
{
	public class CreateSubjectValidator : AbstractValidator<CreateSubjectRequest>
	{
		public CreateSubjectValidator()
		{
			RuleFor(x => x.GradeId)
				.NotEmpty().WithMessage("Grade ID is required");

			RuleFor(x => x.Name)
				.NotEmpty().WithMessage("Name is required")
				.MaximumLength(200).WithMessage("Name must not exceed 200 characters");

			RuleFor(x => x.Description)
				.MaximumLength(1000).WithMessage("Description must not exceed 1000 characters");
		}
	}
}
