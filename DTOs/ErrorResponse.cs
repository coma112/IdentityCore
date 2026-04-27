namespace IdentityCore.DTOs
{
    /// <summary>
    /// A unified error response DTO that is used by all endpoints.
    /// </summary>
    public class ErrorResponse
    {
        public string Message { get; set; } = string.Empty;
        public IEnumerable<string>? Errors { get; set; }

        public ErrorResponse(string message, IEnumerable<string>? errors = null)
        {
            Message = message;
            Errors = errors;
        }
    }
}