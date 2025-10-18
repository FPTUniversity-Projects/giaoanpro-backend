using FluentValidation;
using giaoanpro_backend.Application.DTOs.Requests.SubscriptionPlans;

namespace giaoanpro_backend.Application.Validators.SubscriptionPlans
{
	public class CreateSubscriptionPlanValidator : AbstractValidator<CreateSubscriptionPlanRequest>
	{
		public CreateSubscriptionPlanValidator()
		{
			RuleFor(x => x.Name)
				.NotEmpty().WithMessage("Name is required.")
				.MaximumLength(100).WithMessage("Name must not exceed 100 characters.");
			RuleFor(x => x.Description)
				.MaximumLength(500).WithMessage("Description must not exceed 500 characters.");
			RuleFor(x => x.Price)
				.GreaterThanOrEqualTo(0).WithMessage("Price must be greater than or equal to 0.");
			RuleFor(x => x.DurationInDays)
				.GreaterThan(0).WithMessage("Duration in days must be greater than 0.");
		}
	}
}
