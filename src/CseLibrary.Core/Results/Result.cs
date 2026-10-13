namespace CseLibrary.Core.Results;

/// <summary>Outcome of an expected business operation; unexpected errors are exceptions (D-34).</summary>
public class Result
{
    protected Result(bool succeeded, string? errorCode, string? error)
    {
        Succeeded = succeeded;
        ErrorCode = errorCode;
        Error = error;
    }

    public bool Succeeded { get; }

    /// <summary>Stable code from <see cref="ErrorCodes"/>.</summary>
    public string? ErrorCode { get; }

    /// <summary>Message safe to show to the user.</summary>
    public string? Error { get; }

    public static Result Success() => new(true, null, null);

    public static Result Failure(string errorCode, string error) => new(false, errorCode, error);
}

public sealed class Result<T> : Result
{
    private Result(bool succeeded, T? value, string? errorCode, string? error)
        : base(succeeded, errorCode, error) => Value = value;

    public T? Value { get; }

    public static Result<T> Success(T value) => new(true, value, null, null);

    public static new Result<T> Failure(string errorCode, string error) => new(false, default, errorCode, error);
}
