using FluentValidation;
using giaoanpro_backend.Application.DTOs.Requests.ExamMatrices;

namespace giaoanpro_backend.Application.Validators.ExamMatrices
{
    public class UpdateMatrixRequestValidator : AbstractValidator<UpdateMatrixRequest>
    {
        public UpdateMatrixRequestValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Matrix Id is required.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

            RuleFor(x => x.TotalQuestions)
                .GreaterThan(0).WithMessage("TotalQuestions must be greater than 0.");

            RuleFor(x => x.DurationMinutes)
                .GreaterThanOrEqualTo(0).WithMessage("DurationMinutes must be 0 or greater.");

            RuleFor(x => x.Details)
                .NotNull().WithMessage("Details must be provided.")
                .Must(d => d != null && d.Count > 0).WithMessage("At least one matrix detail is required.");

            RuleForEach(x => x.Details).SetValidator(new MatrixDetailRequestValidator());
        }
    }

    // Small validator for individual matrix lines
    public class MatrixDetailRequestValidator : AbstractValidator<MatrixDetailRequest>
    {
        public MatrixDetailRequestValidator()
        {
            RuleFor(x => x.QuestionType)
                .NotEmpty().WithMessage("QuestionType is required.")
                .MaximumLength(100).WithMessage("QuestionType is too long (max 100 chars).");

            RuleFor(x => x.DifficultyLevel)
                .NotEmpty().WithMessage("DifficultyLevel is required.")
                .MaximumLength(100).WithMessage("DifficultyLevel is too long (max 100 chars).");

            RuleFor(x => x.Count)
                .GreaterThan(0).WithMessage("Count must be greater than 0.");
        }
    }
}
