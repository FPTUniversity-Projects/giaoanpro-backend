using giaoanpro_backend.Application.DTOs.Responses.Subscriptions.Shared;

namespace giaoanpro_backend.Application.DTOs.Responses.Subscriptions
{
	public class GetSubscriptionDetailResponse : GetMySubscriptionDetailResponse
	{
		public UserDetailResponse User { get; set; } = null!;
	}
}
