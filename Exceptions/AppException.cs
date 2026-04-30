namespace IdentityCore.Exceptions
{
    public abstract class AppException : Exception
    {
        public int StatusCode { get; }
        public IEnumerable<string>? Errors { get; }

        protected AppException(int statusCode, string message, IEnumerable<string>? errors = null) : base(message)
        {
            StatusCode = statusCode;
            Errors = errors;
        }
    }
}
