namespace giaoanpro_backend.Application.DTOs.Responses.Subscriptions
{
	public class GetHistorySubscriptionResponse : GetMyHistorySubscriptionResponse
	{
		public string UserEmail { get; set; } = null!;
	}
}
