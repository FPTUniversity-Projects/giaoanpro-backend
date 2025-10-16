using giaoanpro_backend.Domain.Bases;
using giaoanpro_backend.Domain.Enums;

namespace giaoanpro_backend.Domain.Entities
{
	public class Attribute : AuditableEntity
	{
		public Guid Id { get; set; }
		public AttributeType Type { get; set; }
		public TypeValue Value { get; set; }

		// Navigation properties
		public virtual ICollection<QuestionAttribute> QuestionAttributes { get; set; } = new List<QuestionAttribute>();
	}
}
