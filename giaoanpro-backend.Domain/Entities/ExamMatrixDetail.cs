using giaoanpro_backend.Domain.Bases;

namespace giaoanpro_backend.Domain.Entities
{
    public class ExamMatrixDetail : AuditableEntity
    {
        public Guid Id { get; set; }
        public Guid MatrixId { get; set; }
        public string QuestionType { get; set; } = string.Empty;
        public string DifficultyLevel { get; set; } = string.Empty;
        public int Count { get; set; }

        // Navigation
        public virtual ExamMatrix Matrix { get; set; } = null!;
    }
}
