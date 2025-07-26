namespace ShopAI.DTOs
{
    // Standardized API response structure.
    // T represents the Type of the response data
    public class ApiResponse<T>
    {
        // HTTP status code of the response.
        public int StatusCode { get; set; }

        // Indicates whether the request was successful.
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        // Response data in case successful
        public T Data { get; set; }

        // List of error messages, if any.
        public List<string> Errors { get; set; }

        // Optional: Token for authentication purposes.
        public string Token { get; set; } // Add Token property

        public ApiResponse()
        {
            Success = true;
            Errors = new List<string>();
        }

        // Constructor for success responses with data
        public ApiResponse(int statusCode, T data, string token = null)
        {
            StatusCode = statusCode;
            Success = true;
            Data = data;
            Token = token; // Set token if provided
            Errors = new List<string>();
        }

        // Constructor for error responses with error messages
        public ApiResponse(int statusCode, List<string> errors)
        {
            StatusCode = statusCode;
            Success = false;
            Errors = errors;
        }

        // Constructor for error responses with a single error message
        public ApiResponse(int statusCode, string error)
        {
            StatusCode = statusCode;
            Success = false;
            Errors = new List<string> { error };
        }
    }
}
