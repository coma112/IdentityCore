namespace IdentityCore.Exceptions
{
    public class BadRequestException : AppException
    {
        public BadRequestException(string message, IEnumerable<string>? errors = null) : base(StatusCodes.Status400BadRequest, message, errors) { }
    }
}
