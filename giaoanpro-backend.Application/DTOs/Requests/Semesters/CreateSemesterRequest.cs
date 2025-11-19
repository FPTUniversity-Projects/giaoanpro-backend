namespace giaoanpro_backend.Application.DTOs.Requests.Semesters
{
	public class CreateSemesterRequest
	{
		public string Name { get; set; } = string.Empty;
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
	}
}
