using giaoanpro_backend.Domain.Enums;

namespace giaoanpro_backend.Application.DTOs.Requests.Subscriptions
{
	public class CreateSubscriptionRequest
	{
		public Guid UserId { get; set; }
		public Guid PlanId { get; set; }
		public SubscriptionStatus Status { get; set; }
	}
}
