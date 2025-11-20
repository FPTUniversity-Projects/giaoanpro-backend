namespace giaoanpro_backend.Application.DTOs.Requests.ExamMatrices
{
    public class MatrixDetailRequest
    {
        public string QuestionType { get; set; } = string.Empty;
        public string DifficultyLevel { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
