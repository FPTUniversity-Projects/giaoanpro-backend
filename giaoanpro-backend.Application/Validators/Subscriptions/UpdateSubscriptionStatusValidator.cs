using FluentValidation;
using giaoanpro_backend.Application.DTOs.Requests.Subscriptions;

namespace giaoanpro_backend.Application.Validators.Subscriptions
{
	public class UpdateSubscriptionStatusValidator : AbstractValidator<UpdateSubscriptionStatusRequest>
	{
		public UpdateSubscriptionStatusValidator()
		{
			RuleFor(x => x.Status)
				.IsInEnum().WithMessage("Invalid subscription status.");
		}
	}
}
