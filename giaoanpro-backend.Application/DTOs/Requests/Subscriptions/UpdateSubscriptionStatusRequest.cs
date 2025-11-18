using giaoanpro_backend.Domain.Enums;

namespace giaoanpro_backend.Application.DTOs.Requests.Subscriptions
{
	public class UpdateSubscriptionStatusRequest
	{
		public SubscriptionStatus Status { get; set; }
	}
}
