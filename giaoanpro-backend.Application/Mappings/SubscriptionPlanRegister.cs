using giaoanpro_backend.Application.DTOs.Requests.SubscriptionPlans;
using giaoanpro_backend.Application.DTOs.Responses.SubscriptionPlans;
using giaoanpro_backend.Domain.Entities;
using Mapster;

namespace giaoanpro_backend.Application.Mappings
{
	public class SubscriptionPlanRegister : IRegister
	{
		public void Register(TypeAdapterConfig config)
		{
			config.NewConfig<CreateSubscriptionPlanRequest, SubscriptionPlan>()
				.Map(dest => dest.Id, src => Guid.NewGuid())
				.Map(dest => dest.Name, src => src.Name)
				.Map(dest => dest.Description, src => src.Description)
				.Map(dest => dest.Price, src => src.Price)
				.Map(dest => dest.DurationInDays, src => src.DurationInDays)
				.Map(dest => dest.IsActive, src => true);

			config.NewConfig<UpdateSubscriptionPlanRequest, SubscriptionPlan>()
				.Map(dest => dest.Name, src => src.Name)
				.Map(dest => dest.Description, src => src.Description)
				.Map(dest => dest.Price, src => src.Price)
				.Map(dest => dest.DurationInDays, src => src.DurationInDays)
				.Map(dest => dest.IsActive, src => src.IsActive);

			config.NewConfig<SubscriptionPlan, GetSubscriptionPlanResponse>()
				.Map(dest => dest.Id, src => src.Id)
				.Map(dest => dest.Name, src => src.Name)
				.Map(dest => dest.Description, src => src.Description)
				.Map(dest => dest.Price, src => src.Price)
				.Map(dest => dest.DurationInDays, src => src.DurationInDays)
				.Map(dest => dest.IsActive, src => src.IsActive);
		}
	}
}
