using giaoanpro_backend.Domain.Enums;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace giaoanpro_backend.Application.DTOs.Requests.Materials
{
	public class UploadMaterialRequest
	{
		[Required(ErrorMessage = "ActivityId is required")]
		public Guid ActivityId { get; set; }

		[Required(ErrorMessage = "Title is required")]
		[MaxLength(255, ErrorMessage = "Title cannot exceed 255 characters")]
		public string Title { get; set; } = string.Empty;

		[Required(ErrorMessage = "Type is required")]
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public MaterialType Type { get; set; }

		[Required(ErrorMessage = "File is required")]
		public IFormFile File { get; set; } = null!;
	}
}
