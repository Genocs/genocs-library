using System.Net;

namespace Genocs.Persistence.EFCore.Exceptions;

public class ConflictException(string message) : CustomException(message, null, HttpStatusCode.Conflict);
