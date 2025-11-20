using System.Linq;
using FluentValidation;
using giaoanpro_backend.Application.DTOs.Requests.Exams;

namespace giaoanpro_backend.Application.Validators.Exams
{
    public class CreateExamValidator : AbstractValidator<CreateExamRequest>
    {
        public CreateExamValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

            RuleFor(x => x.MatrixId)
                .NotEmpty().WithMessage("MatrixId is required.");

            RuleFor(x => x.QuestionIds)
                .NotNull().WithMessage("QuestionIds must be provided.")
                .Must(list => list != null && list.Any()).WithMessage("At least one question must be selected.")
                .Must(list => list == null || list.Distinct().Count() == list.Count)
                .WithMessage("QuestionIds must be unique (no duplicates).");
        }
    }
}
