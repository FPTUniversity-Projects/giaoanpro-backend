using FluentValidation;
using giaoanpro_backend.Application.DTOs.Requests.Subscriptions;

namespace giaoanpro_backend.Application.Validators.Subscriptions
{
	public class CreateSubscriptionValidator : AbstractValidator<CreateSubscriptionRequest>
	{
		public CreateSubscriptionValidator()
		{
			RuleFor(x => x.PlanId)
				.NotEmpty().WithMessage("Subscription plan ID is required.")
				.Must(id => id != Guid.Empty).WithMessage("Subscription plan ID must be a valid GUID.");
			RuleFor(x => x.UserId)
				.NotEmpty().WithMessage("User ID is required.")
				.Must(id => id != Guid.Empty).WithMessage("User ID must be a valid GUID.");
			RuleFor(x => x.Status)
				.NotEmpty().WithMessage("Subscription status is required.")
				.IsInEnum().WithMessage("Subscription status must be a valid enum value.");
		}
	}
}
