using giaoanpro_backend.Application.DTOs.Requests.Bases;

namespace giaoanpro_backend.Application.DTOs.Requests.SubscriptionPlans
{
	public class GetSubscriptionPlansQuery : PagingAndSortingQuery
	{
		public string? Search { get; set; }
		public decimal? MinPrice { get; set; }
		public decimal? MaxPrice { get; set; }
		public int? MinDurationInDays { get; set; }
		public int? MaxDurationInDays { get; set; }
		public int? MinLessons { get; set; }
		public int? MinPromptsPerDay { get; set; }
		public bool? IsActive { get; set; }
	}
}
