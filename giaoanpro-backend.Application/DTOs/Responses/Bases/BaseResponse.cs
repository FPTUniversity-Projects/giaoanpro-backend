namespace giaoanpro_backend.Application.DTOs.Responses.Bases
{
	public class BaseResponse
	{
		public bool Success { get; set; } = true;
		public string Message { get; set; } = "OK";
		public List<string>? Errors { get; set; }
		public ResponseErrorType ErrorType { get; set; } = ResponseErrorType.None;

		public static BaseResponse Ok(string message = "Success")
			=> new() { Success = true, Message = message };

		public static BaseResponse Fail(string message, ResponseErrorType errorType, List<string>? errors = null)
		=> new() { Success = false, Message = message, ErrorType = errorType, Errors = errors };

		[Obsolete("Vui lòng sử dụng hàm Fail(message, errorType) mới. Hàm này sẽ mặc định lỗi là BadRequest.")]
		public static BaseResponse Fail(string message, List<string>? errors = null)
			=> Fail(message, ResponseErrorType.BadRequest, errors);
	}

	public class BaseResponse<T> : BaseResponse
	{
		public T? Payload { get; set; }

		public static BaseResponse<T> Ok(T data, string message = "Success")
			=> new() { Success = true, Message = message, Payload = data };

		public static new BaseResponse<T> Fail(string message, ResponseErrorType errorType, List<string>? errors = null)
			=> new() { Success = false, Message = message, ErrorType = errorType, Errors = errors };

		[Obsolete("Vui lòng sử dụng hàm Fail(message, errorType) mới. Hàm này sẽ mặc định lỗi là BadRequest.")]
		public static new BaseResponse<T> Fail(string message, List<string>? errors = null)
			=> Fail(message, ResponseErrorType.BadRequest, errors);
	}
}
