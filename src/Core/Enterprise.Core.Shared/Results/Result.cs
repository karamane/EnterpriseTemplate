namespace Enterprise.Core.Shared.Results;

/// <summary>
/// İşlem sonucunu temsil eden generic result pattern implementasyonu
/// </summary>
/// <typeparam name="T">Başarılı sonuçta döndürülecek veri tipi</typeparam>
public class Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T? Data { get; }
    public string? Message { get; }
    public string? ErrorCode { get; }
    public List<string> Errors { get; } = new();

    protected Result(bool isSuccess, T? data, string? message, string? errorCode = null)
    {
        IsSuccess = isSuccess;
        Data = data;
        Message = message;
        ErrorCode = errorCode;
    }

    /// <summary>
    /// Başarılı sonuç oluşturur
    /// </summary>
    public static Result<T> Success(T data, string? message = null)
        => new(true, data, message);

    /// <summary>
    /// Başarısız sonuç oluşturur
    /// </summary>
    public static Result<T> Failure(string message, string? errorCode = null)
        => new(false, default, message, errorCode);

    /// <summary>
    /// Validation hatalı sonuç oluşturur
    /// </summary>
    public static Result<T> ValidationFailure(List<string> errors)
    {
        var result = new Result<T>(false, default, "Validation failed", "VALIDATION_ERROR");
        result.Errors.AddRange(errors);
        return result;
    }

    /// <summary>
    /// Implicit conversion to bool
    /// </summary>
    public static implicit operator bool(Result<T> result) => result.IsSuccess;
}

/// <summary>
/// Veri içermeyen işlem sonucu
/// </summary>
public class Result : Result<object>
{
    private Result(bool isSuccess, string? message, string? errorCode = null)
        : base(isSuccess, null, message, errorCode)
    {
    }

    public new static Result Success(string? message = null)
        => new(true, message);

    public new static Result Failure(string message, string? errorCode = null)
        => new(false, message, errorCode);
}

