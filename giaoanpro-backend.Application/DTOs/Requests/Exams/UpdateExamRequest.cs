namespace giaoanpro_backend.Application.DTOs.Requests.Exams
{
	public class UpdateExamRequest
	{
		//public Guid Id { get; set; }
		public string Title { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public List<Guid> QuestionIds { get; set; } = new List<Guid>();
	}
}
