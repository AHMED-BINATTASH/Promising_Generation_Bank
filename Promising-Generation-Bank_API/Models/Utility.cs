namespace Promising_Generation_Bank_API.Models
{
    public class ApiResponse<T>
    {
        public T? Data { get; set; }
        public string Message { get; set; }
        public string Code { get; set; }

        public static ApiResponse<T> SuccessResponse(T? data = default, string message = "Success", string code = "SUCCESS")
        {
            return new ApiResponse<T>
            {
                Data = data,
                Message = message,
                Code = code
            };

        }
        public static ApiResponse<T> FailureResponse(string message, string code = "ERROR")
        {
            return new ApiResponse<T>
            {
                Message = message,
                Code = code
            };

        }
    }

    public static class ResultCode
    {
        public const string Success = "SUCCESS";
        public const string Created = "CREATED_SUCCESSFULLY";
        public const string Updated = "UPDATED_SUCCESSFULLY";
        public const string Deleted = "DELETED_SUCCESSFULLY";

        public const string InvalidRequest = "INVALID_REQUEST";
        public const string BadRequest = "BAD_REQUEST";
        public const string ValidationError = "VALIDATION_ERROR";
        public const string NotFound = "NOT_FOUND";
        public const string Found = "FOUND";

        public const string AlreadyExists = "ALREADY_EXISTS"
            ;
        public const string TokenExpired = "TOKEN_EXPIRED";
        public const string Unauthorized = "UNAUTHORIZED";
        public const string Forbidden = "FORBIDDEN";

        public const string InternalError = "INTERNAL_SERVER_ERROR";
        public const string ServerWork = "SERVER_WORK";
    }
}
