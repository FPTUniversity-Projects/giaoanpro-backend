using FluentValidation;
using giaoanpro_backend.Application.DTOs.Requests.Users;

namespace giaoanpro_backend.Application.Validators.Users
{
	public class UpdateCurrentUserValidator : AbstractValidator<UpdateCurrentUserRequest>
	{
		public UpdateCurrentUserValidator()
		{
			RuleFor(x => x.FullName).MaximumLength(200);
			RuleFor(x => x.Username).NotEmpty().When(x => !string.IsNullOrWhiteSpace(x.Username)).MinimumLength(3).MaximumLength(50);
		}
	}
}
