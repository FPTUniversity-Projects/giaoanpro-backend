namespace giaoanpro_backend.Application.DTOs.Responses.Questions
{
	public class GetQuestionsPagedResponse
	{
		public List<GetQuestionResponse> Items { get; set; } = new();
		public int PageNumber { get; set; }
		public int PageSize { get; set; }
		public int TotalCount { get; set; }
		public int TotalPages { get; set; }
		public bool HasPreviousPage { get; set; }
		public bool HasNextPage { get; set; }
	}
}
