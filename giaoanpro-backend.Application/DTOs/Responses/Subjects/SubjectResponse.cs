namespace giaoanpro_backend.Application.DTOs.Responses.Subjects
{
	public class SubjectResponse
	{
		public Guid Id { get; set; }
		public Guid GradeId { get; set; }
		public string GradeLevel { get; set; } = string.Empty;
		public string Name { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }
	}
}
