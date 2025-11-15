namespace giaoanpro_backend.Application.DTOs.Requests.Subscriptions
{
	public class GetSubscriptionsQuery : GetMySubscriptionHistoryQuery
	{
		/// <summary>
		/// Tìm kiếm theo văn bản tự do.
		/// (Service sẽ xử lý tìm trong User.Name, User.Email, Plan.Name, v.v.)
		/// </summary>
		public string? Search { get; set; }

		/// <summary>
		/// Lọc theo một UserId cụ thể. (Chỉ dành cho Admin)
		/// </summary>
		public Guid? UserId { get; set; }

		// === BỘ LỌC NGÀY THÁNG (DATE FILTERING) ===

		/// <summary>
		/// Lọc các gói đăng ký SẼ HẾT HẠN "TRƯỚC" ngày này.
		/// </summary>
		public DateTime? ExpiresBefore { get; set; }

		/// <summary>
		/// Lọc các gói đăng ký SẼ HẾT HẠN "SAU" ngày này.
		/// </summary>
		public DateTime? ExpiresAfter { get; set; }

		// === BỘ LỌC SỬ DỤNG (USAGE FILTERING) ===

		/// <summary>
		/// Lọc các gói có số prompt đã dùng "ÍT NHẤT" là con số này.
		/// </summary>
		public int? MinPromptsUsed { get; set; }

		/// <summary>
		/// Lọc các gói có số giáo án đã tạo "ÍT NHẤT" là con số này.
		/// </summary>
		public int? MinLessonsCreated { get; set; }
	}
}
