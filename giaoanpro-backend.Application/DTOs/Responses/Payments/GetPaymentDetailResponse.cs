namespace giaoanpro_backend.Application.DTOs.Responses.Payments
{
	public class GetPaymentDetailResponse : GetPaymentResponse
	{
		public SubscriptionDto? Subscription { get; set; }
	}

	public class SubscriptionDto
	{
		public Guid Id { get; set; }
		public string UserFullName { get; set; } = string.Empty;
		public string PlanName { get; set; } = string.Empty;
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public string Status { get; set; } = string.Empty;
	}
}
