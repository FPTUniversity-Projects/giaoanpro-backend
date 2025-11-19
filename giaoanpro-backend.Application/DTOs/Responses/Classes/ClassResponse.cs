namespace giaoanpro_backend.Application.DTOs.Responses.Classes
{
	public class ClassResponse
	{
		public Guid Id { get; set; }
		public Guid TeacherId { get; set; }
		public string TeacherName { get; set; } = string.Empty;
		public Guid GradeId { get; set; }
		public int GradeLevel { get; set; }
		public Guid SemesterId { get; set; }
		public string SemesterName { get; set; } = string.Empty;
		public string Name { get; set; } = string.Empty;
		public int MemberCount { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }
	}
}
