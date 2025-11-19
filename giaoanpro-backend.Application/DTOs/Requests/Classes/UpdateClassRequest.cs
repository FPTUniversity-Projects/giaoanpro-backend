namespace giaoanpro_backend.Application.DTOs.Requests.Classes
{
	public class UpdateClassRequest
	{
		public Guid TeacherId { get; set; }
		public Guid GradeId { get; set; }
		public Guid SemesterId { get; set; }
		public string Name { get; set; } = string.Empty;
	}
}
