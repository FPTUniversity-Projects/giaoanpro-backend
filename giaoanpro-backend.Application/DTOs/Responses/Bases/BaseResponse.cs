namespace giaoanpro_backend.Application.DTOs.Responses.Bases
{
	public class BaseResponse
	{
		public bool Success { get; set; } = true;
		public string Message { get; set; } = "OK";
		public List<string>? Errors { get; set; }

		public static BaseResponse Ok(string message = "Success")
			=> new() { Success = true, Message = message };

		public static BaseResponse Fail(string message, List<string>? errors = null)
			=> new() { Success = false, Message = message, Errors = errors };
	}

	public class BaseResponse<T> : BaseResponse
	{
		public T? Data { get; set; }

		public static BaseResponse<T> Ok(T data, string message = "Success")
			=> new() { Success = true, Message = message, Data = data };

		public static new BaseResponse<T> Fail(string message, List<string>? errors = null)
			=> new() { Success = false, Message = message, Errors = errors };
	}
}
