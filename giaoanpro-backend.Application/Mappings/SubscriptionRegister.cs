using giaoanpro_backend.Application.DTOs.Responses.Subscriptions;
using giaoanpro_backend.Application.DTOs.Responses.Subscriptions.Shared;
using giaoanpro_backend.Domain.Entities;
using Mapster;

namespace giaoanpro_backend.Application.Mappings
{
	public class SubscriptionRegister : IRegister
	{
		public void Register(TypeAdapterConfig config)
		{
			// Map nested entities used in subscription DTOs
			config.NewConfig<SubscriptionPlan, SubscriptionPlanDetailResponse>()
				.Map(dest => dest.Id, src => src.Id)
				.Map(dest => dest.Name, src => src.Name)
				.Map(dest => dest.Description, src => src.Description)
				.Map(dest => dest.Price, src => src.Price)
				.Map(dest => dest.DurationInDays, src => src.DurationInDays)
				.Map(dest => dest.MaxLessonPlans, src => src.MaxLessonPlans)
				.Map(dest => dest.MaxPromptsPerDay, src => src.MaxPromptsPerDay);

			config.NewConfig<Payment, PaymentDetailResponse>()
				.Map(dest => dest.Id, src => src.Id)
				.Map(dest => dest.SubscriptionId, src => src.SubscriptionId)
				.Map(dest => dest.Amount, src => src.AmountPaid)
				.Map(dest => dest.PaymentDate, src => src.PaymentDate)
				.Map(dest => dest.PaymentMethod, src => src.PaymentMethod)
				.Map(dest => dest.Status, src => src.Status.ToString());

			config.NewConfig<User, UserDetailResponse>()
				.Map(dest => dest.Id, src => src.Id)
				.Map(dest => dest.Email, src => src.Email)
				.Map(dest => dest.FullName, src => src.FullName);

			config.NewConfig<Subscription, GetMyHistorySubscriptionResponse>()
				.Map(dest => dest.Status, src => src.Status.ToString())
				.Map(dest => dest.Id, src => src.Id)
				.Map(dest => dest.PlanId, src => src.PlanId)
				.Map(dest => dest.PlanName, src => src.Plan != null ? src.Plan.Name : string.Empty)
				.Map(dest => dest.StartDate, src => src.StartDate)
				.Map(dest => dest.EndDate, src => src.EndDate);

			config.NewConfig<Subscription, GetMyCurrentAccessResponse>()
				.Map(dest => dest.Status, src => src.Status.ToString())
				.Map(dest => dest.Id, src => src.Id)
				.Map(dest => dest.PlanId, src => src.PlanId)
				.Map(dest => dest.PlanName, src => src.Plan != null ? src.Plan.Name : string.Empty)
				.Map(dest => dest.StartDate, src => src.StartDate)
				.Map(dest => dest.EndDate, src => src.EndDate)
				.Map(dest => dest.CurrentLessonPlansCreated, src => src.CurrentLessonPlansCreated)
				.Map(dest => dest.CurrentPromptsUsed, src => src.CurrentPromptsUsed)
				.Map(dest => dest.LastPromptResetDate, src => src.LastPromptResetDate);

			config.NewConfig<Subscription, GetMySubscriptionDetailResponse>()
				.Map(dest => dest.Id, src => src.Id)
				.Map(dest => dest.StartDate, src => src.StartDate)
				.Map(dest => dest.EndDate, src => src.EndDate)
				.Map(dest => dest.Status, src => src.Status.ToString())
				.Map(dest => dest.CurrentLessonPlansCreated, src => src.CurrentLessonPlansCreated)
				.Map(dest => dest.CurrentPromptsUsed, src => src.CurrentPromptsUsed)
				.Map(dest => dest.LastPromptResetDate, src => src.LastPromptResetDate)
				.Map(dest => dest.Plan, src => src.Plan == null ? null : src.Plan.Adapt<SubscriptionPlanDetailResponse>())
				.Map(dest => dest.Payments, src => src.Payments.Select(p => p.Adapt<PaymentDetailResponse>()).ToList());

			config.NewConfig<Subscription, GetSubscriptionDetailResponse>()
				.Map(dest => dest.Id, src => src.Id)
				.Map(dest => dest.StartDate, src => src.StartDate)
				.Map(dest => dest.EndDate, src => src.EndDate)
				.Map(dest => dest.Status, src => src.Status.ToString())
				.Map(dest => dest.CurrentLessonPlansCreated, src => src.CurrentLessonPlansCreated)
				.Map(dest => dest.CurrentPromptsUsed, src => src.CurrentPromptsUsed)
				.Map(dest => dest.LastPromptResetDate, src => src.LastPromptResetDate)
				.Map(dest => dest.Plan, src => src.Plan == null ? null : src.Plan.Adapt<SubscriptionPlanDetailResponse>())
				.Map(dest => dest.Payments, src => src.Payments.Select(p => p.Adapt<PaymentDetailResponse>()).ToList())
				.Map(dest => dest.User, src => src.User == null ? null : src.User.Adapt<UserDetailResponse>());

			config.NewConfig<Subscription, GetHistorySubscriptionResponse>()
				.Map(dest => dest.Status, src => src.Status.ToString())
				.Map(dest => dest.Id, src => src.Id)
				.Map(dest => dest.PlanId, src => src.PlanId)
				.Map(dest => dest.PlanName, src => src.Plan != null ? src.Plan.Name : string.Empty)
				.Map(dest => dest.StartDate, src => src.StartDate)
				.Map(dest => dest.EndDate, src => src.EndDate)
				.Map(dest => dest.UserEmail, src => src.User != null ? src.User.Email : string.Empty);

		}
	}
}
