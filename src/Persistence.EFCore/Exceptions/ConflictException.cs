using System.Net;

namespace Genocs.Persistence.EFCore.Exceptions;

public class ConflictException : CustomException
{
    public ConflictException(string message)
        : base(message, null, HttpStatusCode.Conflict)
    {
    }

    public ConflictException(string message, Exception innerException)
        : base(message, null, HttpStatusCode.Conflict, innerException)
    {
    }
}
