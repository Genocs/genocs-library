namespace Genocs.Common.Types;

/// <summary>
/// Represents an error with a code and message.
/// </summary>
public sealed record Error(string Code, string Message);

/// <summary>
/// Represents the result of an operation, indicating success or failure.
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public Error? Error { get; }

    public bool IsFailure => !IsSuccess;

    protected Result(bool isSuccess, Error? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, null);
    public static Result Failure(Error error) => new(false, error);
}

/// <summary>
/// Represents the result of an operation with a value, indicating success or failure.
/// </summary>
public sealed class Result<T> : Result
{
    public T Value { get; }

    private Result(bool isSuccess, T value, Error? error)
        : base(isSuccess, error)
    {
        Value = value;
    }

    public static Result<T> Success(T value) => new(true, value, null);
    public static new Result<T> Failure(Error error) => new(false, default!, error);
}
