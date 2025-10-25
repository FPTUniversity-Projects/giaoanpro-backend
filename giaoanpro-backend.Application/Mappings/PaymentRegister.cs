using giaoanpro_backend.Application.DTOs.Responses.Payments;
using giaoanpro_backend.Domain.Entities;
using Mapster;

namespace giaoanpro_backend.Application.Mappings
{
	public class PaymentRegister : IRegister
	{
		public void Register(TypeAdapterConfig config)
		{
			config.NewConfig<Payment, GetPaymentResponse>()
				.Map(dest => dest.Id, src => src.Id)
				.Map(dest => dest.SubscriptionId, src => src.SubscriptionId)
				.Map(dest => dest.PaymentDate, src => src.PaymentDate)
				.Map(dest => dest.AmountPaid, src => src.AmountPaid)
				.Map(dest => dest.PaymentMethod, src => src.PaymentMethod)
				.Map(dest => dest.Status, src => src.Status)
				.Map(dest => dest.Description, src => src.Description);

			config.NewConfig<Payment, GetPaymentDetailResponse>()
				.Map(dest => dest.Id, src => src.Id)
				.Map(dest => dest.SubscriptionId, src => src.SubscriptionId)
				.Map(dest => dest.PaymentDate, src => src.PaymentDate)
				.Map(dest => dest.AmountPaid, src => src.AmountPaid)
				.Map(dest => dest.PaymentMethod, src => src.PaymentMethod)
				.Map(dest => dest.Status, src => src.Status)
				.Map(dest => dest.Description, src => src.Description)
				.Map(dest => dest.Subscription, src => src.Subscription == null
					? null
					: new SubscriptionDto
					{
						Id = src.Subscription.Id,
						UserFullName = src.Subscription.User != null ? src.Subscription.User.FullName ?? string.Empty : string.Empty,
						PlanName = src.Subscription.Plan != null ? src.Subscription.Plan.Name ?? string.Empty : string.Empty,
						StartDate = src.Subscription.StartDate,
						EndDate = src.Subscription.EndDate,
						Status = src.Subscription.Status.ToString()
					});
		}
	}
}
