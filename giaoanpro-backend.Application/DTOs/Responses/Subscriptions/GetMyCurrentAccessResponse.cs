namespace giaoanpro_backend.Application.DTOs.Responses.Subscriptions
{
	public class GetMyCurrentAccessResponse : GetMyHistorySubscriptionResponse
	{
		public int CurrentLessonPlansCreated { get; set; }
		public int CurrentPromptsUsed { get; set; }
		public DateTime? LastPromptResetDate { get; set; }
	}
}
