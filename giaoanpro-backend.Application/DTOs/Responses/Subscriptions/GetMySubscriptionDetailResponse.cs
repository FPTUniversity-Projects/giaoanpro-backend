using giaoanpro_backend.Application.DTOs.Responses.Subscriptions.Shared;

namespace giaoanpro_backend.Application.DTOs.Responses.Subscriptions
{
	public class GetMySubscriptionDetailResponse
	{
		public Guid Id { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public string Status { get; set; } = null!;

		// Usage limits
		public int CurrentLessonPlansCreated { get; set; }
		public int CurrentPromptsUsed { get; set; }
		public DateTime? LastPromptResetDate { get; set; }

		public SubscriptionPlanDetailResponse Plan { get; set; } = null!;

		public List<PaymentDetailResponse> Payments { get; set; } = new List<PaymentDetailResponse>();
	}
}
