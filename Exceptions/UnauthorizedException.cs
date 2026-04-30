namespace IdentityCore.Exceptions
{
    public class UnauthorizedException : AppException
    {
        public UnauthorizedException(string message = "Unauthorized.") : base(StatusCodes.Status401Unauthorized, message) { }
    }
}
