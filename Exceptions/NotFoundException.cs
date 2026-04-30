namespace IdentityCore.Exceptions
{
    public class NotFoundException : AppException
    {
        public NotFoundException(string message = "Resource not found.") : base(StatusCodes.Status404NotFound, message) { }
    }
}
