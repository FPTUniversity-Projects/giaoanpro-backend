using giaoanpro_backend.Domain.Enums;
using System.Text.Json.Serialization;

namespace giaoanpro_backend.Application.DTOs.Requests.Attributes
{
	public class CreateAttributeRequest
	{
		public string Type { get; set; } = string.Empty;
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public TypeValue Value { get; set; }
	}
}
