namespace giaoanpro_backend.Application.DTOs.Responses.Syllabuses
{
	public class SyllabusResponse
	{
		public Guid Id { get; set; }
		public Guid SubjectId { get; set; }
		public string SubjectName { get; set; } = string.Empty;
		public string Name { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public string? PdfUrl { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }
	}
}
