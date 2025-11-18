namespace giaoanpro_backend.Application.DTOs.Requests.Syllabuses
{
	public class CreateSyllabusRequest
	{
		public Guid SubjectId { get; set; }
		public string Name { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public string? PdfUrl { get; set; }
	}
}
