using FluentValidation;
using giaoanpro_backend.Application.DTOs.Requests.Exams;

namespace giaoanpro_backend.Application.Validators.Exams
{
    public class GenerateQuestionPromptRequestValidator : AbstractValidator<GenerateQuestionPromptRequest>
    {
        // Define allowed values to keep things strict
        private readonly string[] _allowedDifficulties = { "Easy", "Medium", "Hard" };
        private readonly string[] _allowedTypes = { "Theory", "Exercise" };

        public GenerateQuestionPromptRequestValidator()
        {
            RuleFor(x => x.MatrixId)
                .NotEmpty().WithMessage("MatrixId is required.");

            // 1. Count: Lower limit to prevent Timeouts
            RuleFor(x => x.Count)
                .GreaterThan(0).WithMessage("Count must be greater than 0.")
                .LessThanOrEqualTo(20).WithMessage("You can only generate up to 20 questions at a time to ensure speed.");

            // 2. Topic: Good as is
            RuleFor(x => x.Topic)
                .NotEmpty().WithMessage("Topic is required.")
                .MaximumLength(500).WithMessage("Topic is too long (max 500 chars).");

            // 3. Difficulty: Strict check
            RuleFor(x => x.DifficultyLevel)
                .NotEmpty()
                .Must(level => _allowedDifficulties.Contains(level))
                .WithMessage($"Difficulty must be one of: {string.Join(", ", _allowedDifficulties)}");

            // 4. Question Type: Strict check
            RuleFor(x => x.QuestionType)
                .NotEmpty()
                .Must(type => _allowedTypes.Contains(type))
                .WithMessage($"Question Type must be one of: {string.Join(", ", _allowedTypes)}");
        }
    }
}
