namespace Tervia.Core;

public readonly record struct TvResult<T>
{
    private TvResult(T? value, string? error)
    {
        Value = value;
        Error = error;
    }

    public T? Value { get; }
    public string? Error { get; }
    public bool IsSuccess => Error is null;

    public static TvResult<T> Ok(T value) => new(value, null);
    public static TvResult<T> Fail(string error) => new(default, error);
}
