namespace giaoanpro_backend.Application.DTOs.Responses.Subscriptions
{
	public class GetSubscriptionDetailResponse
	{
		public Guid Id { get; set; }
		public Guid UserId { get; set; }
		public Guid PlanId { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public string Status { get; set; } = null!;

		// plan details
		public SubscriptionPlanDetailResponse Plan { get; set; } = null!;

		// payments 
		public List<PaymentDetailResponse> Payments { get; set; } = new List<PaymentDetailResponse>();
	}

	public class SubscriptionPlanDetailResponse
	{
		public Guid Id { get; set; }
		public string Name { get; set; } = null!;
		public string Description { get; set; } = null!;
		public decimal Price { get; set; }
		public int DurationInDays { get; set; }
	}

	public class PaymentDetailResponse
	{
		public Guid Id { get; set; }
		public Guid SubscriptionId { get; set; }
		public decimal Amount { get; set; }
		public DateTime PaymentDate { get; set; }
		public string PaymentMethod { get; set; } = null!;
		public string Status { get; set; } = null!;
	}
}
