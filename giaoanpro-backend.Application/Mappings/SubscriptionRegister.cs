using giaoanpro_backend.Application.DTOs.Responses.Subscriptions;
using giaoanpro_backend.Domain.Entities;
using Mapster;

namespace giaoanpro_backend.Application.Mappings
{
	public class SubscriptionRegister : IRegister
	{
		public void Register(TypeAdapterConfig config)
		{
			config.NewConfig<Subscription, GetSubscriptionResponse>()
				.Map(dest => dest.Status, src => src.Status.ToString())
				.Map(dest => dest.Id, src => src.Id)
				.Map(dest => dest.UserId, src => src.UserId)
				.Map(dest => dest.PlanId, src => src.PlanId)
				.Map(dest => dest.StartDate, src => src.StartDate)
				.Map(dest => dest.EndDate, src => src.EndDate);

			config.NewConfig<Subscription, GetSubscriptionDetailResponse>()
				.Map(dest => dest.Status, src => src.Status.ToString())
				.Map(dest => dest.Id, src => src.Id)
				.Map(dest => dest.UserId, src => src.UserId)
				.Map(dest => dest.PlanId, src => src.PlanId)
				.Map(dest => dest.StartDate, src => src.StartDate)
				.Map(dest => dest.EndDate, src => src.EndDate)
				.Map(dest => dest.Plan, src => src.Plan)
				.Map(dest => dest.Payments, src => src.Payments);
		}
	}
}
