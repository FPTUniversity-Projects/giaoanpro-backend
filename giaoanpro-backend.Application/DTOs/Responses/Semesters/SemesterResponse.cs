namespace giaoanpro_backend.Application.DTOs.Responses.Semesters
{
	public class SemesterResponse
	{
		public Guid Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }
	}
}
