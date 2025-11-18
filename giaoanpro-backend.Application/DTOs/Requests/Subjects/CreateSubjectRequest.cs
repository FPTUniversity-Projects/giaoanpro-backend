namespace giaoanpro_backend.Application.DTOs.Requests.Subjects
{
	public class CreateSubjectRequest
	{
		public Guid GradeId { get; set; }
		public string Name { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
	}
}
