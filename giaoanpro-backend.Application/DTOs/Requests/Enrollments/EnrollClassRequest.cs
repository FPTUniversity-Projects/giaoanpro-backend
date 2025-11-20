namespace giaoanpro_backend.Application.DTOs.Requests.Enrollments
{
	public class EnrollClassRequest
	{
		public required Guid ClassId { get; set; }
		public Guid? StudentId { get; set; }
	}
}
