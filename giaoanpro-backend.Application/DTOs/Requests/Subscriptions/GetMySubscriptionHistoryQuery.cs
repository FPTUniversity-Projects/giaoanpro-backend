using giaoanpro_backend.Application.DTOs.Requests.Bases;
using giaoanpro_backend.Domain.Enums;

namespace giaoanpro_backend.Application.DTOs.Requests.Subscriptions
{
	public class GetMySubscriptionHistoryQuery : PagingAndSortingQuery
	{
		/// <summary>
		/// Lọc theo một PlanId cụ thể.
		/// (Hữu ích khi user muốn xem: "Tôi đã mua Gói Pro bao nhiêu lần?")
		/// </summary>
		public Guid? PlanId { get; set; }

		/// <summary>
		/// Lọc theo trạng thái.
		/// (Hữu ích khi user muốn xem: "Các gói đã hết hạn của tôi")
		/// </summary>
		public SubscriptionStatus? Status { get; set; }
	}
}
