using FluentValidation;
using giaoanpro_backend.Application.DTOs.Requests.Syllabuses;

namespace giaoanpro_backend.Application.Validators.Syllabuses
{
	public class UpdateSyllabusValidator : AbstractValidator<UpdateSyllabusRequest>
	{
		public UpdateSyllabusValidator()
		{
			RuleFor(x => x.SubjectId)
				.NotEmpty().WithMessage("Subject ID is required");

			RuleFor(x => x.Name)
				.NotEmpty().WithMessage("Name is required")
				.MaximumLength(200).WithMessage("Name must not exceed 200 characters");

			RuleFor(x => x.Description)
				.MaximumLength(1000).WithMessage("Description must not exceed 1000 characters");

			RuleFor(x => x.PdfUrl)
				.MaximumLength(500).WithMessage("PDF URL must not exceed 500 characters");
		}
	}
}
