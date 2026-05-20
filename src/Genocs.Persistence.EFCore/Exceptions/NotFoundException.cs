using System.Net;

namespace Genocs.Persistence.EFCore.Exceptions;

public class NotFoundException(string message) : CustomException(message, null, HttpStatusCode.NotFound);
