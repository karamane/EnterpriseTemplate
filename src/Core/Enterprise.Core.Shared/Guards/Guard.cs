using Enterprise.Core.Shared.Exceptions;

namespace Enterprise.Core.Shared.Guards;

/// <summary>
/// Guard clause implementasyonu - Defensive programming için
/// </summary>
public static class Guard
{
    /// <summary>
    /// Değerin null olmadığını kontrol eder
    /// </summary>
    public static T AgainstNull<T>(T? value, string parameterName) where T : class
    {
        if (value is null)
            throw new ArgumentNullException(parameterName, $"{parameterName} cannot be null.");
        
        return value;
    }

    /// <summary>
    /// String'in null veya boş olmadığını kontrol eder
    /// </summary>
    public static string AgainstNullOrEmpty(string? value, string parameterName)
    {
        if (string.IsNullOrEmpty(value))
            throw new ArgumentException($"{parameterName} cannot be null or empty.", parameterName);
        
        return value;
    }

    /// <summary>
    /// String'in null, boş veya whitespace olmadığını kontrol eder
    /// </summary>
    public static string AgainstNullOrWhiteSpace(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{parameterName} cannot be null, empty, or whitespace.", parameterName);
        
        return value;
    }

    /// <summary>
    /// Sayının negatif olmadığını kontrol eder
    /// </summary>
    public static int AgainstNegative(int value, string parameterName)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(parameterName, value, $"{parameterName} cannot be negative.");
        
        return value;
    }

    /// <summary>
    /// Sayının sıfır veya negatif olmadığını kontrol eder
    /// </summary>
    public static int AgainstNegativeOrZero(int value, string parameterName)
    {
        if (value <= 0)
            throw new ArgumentOutOfRangeException(parameterName, value, $"{parameterName} must be greater than zero.");
        
        return value;
    }

    /// <summary>
    /// Değerin belirtilen aralıkta olduğunu kontrol eder
    /// </summary>
    public static int AgainstOutOfRange(int value, string parameterName, int min, int max)
    {
        if (value < min || value > max)
            throw new ArgumentOutOfRangeException(parameterName, value, 
                $"{parameterName} must be between {min} and {max}.");
        
        return value;
    }

    /// <summary>
    /// Koleksiyonun null veya boş olmadığını kontrol eder
    /// </summary>
    public static IEnumerable<T> AgainstNullOrEmpty<T>(IEnumerable<T>? value, string parameterName)
    {
        if (value is null || !value.Any())
            throw new ArgumentException($"{parameterName} cannot be null or empty.", parameterName);
        
        return value;
    }

    /// <summary>
    /// GUID'in boş olmadığını kontrol eder
    /// </summary>
    public static Guid AgainstEmpty(Guid value, string parameterName)
    {
        if (value == Guid.Empty)
            throw new ArgumentException($"{parameterName} cannot be empty GUID.", parameterName);
        
        return value;
    }

    /// <summary>
    /// Koşulun doğru olduğunu kontrol eder, değilse business exception fırlatır
    /// </summary>
    public static void AgainstBusinessRule(bool condition, string errorMessage, string errorCode = "BUSINESS_RULE_VIOLATION")
    {
        if (condition)
            throw new BusinessException(errorMessage, errorCode);
    }

    /// <summary>
    /// Kaynağın bulunduğunu kontrol eder
    /// </summary>
    public static T AgainstNotFound<T>(T? value, string resourceType, string resourceId) where T : class
    {
        if (value is null)
            throw new NotFoundException(resourceType, resourceId);
        
        return value;
    }

    /// <summary>
    /// String uzunluğunun maksimum değeri aşmadığını kontrol eder
    /// </summary>
    public static string AgainstMaxLength(string? value, int maxLength, string parameterName)
    {
        if (!string.IsNullOrEmpty(value) && value.Length > maxLength)
            throw new ArgumentException($"{parameterName} cannot exceed {maxLength} characters.", parameterName);
        
        return value ?? string.Empty;
    }

    /// <summary>
    /// Email formatının geçerli olduğunu kontrol eder
    /// </summary>
    public static string AgainstInvalidEmail(string? value, string parameterName)
    {
        AgainstNullOrWhiteSpace(value, parameterName);
        
        if (!value!.Contains('@') || !value.Contains('.'))
            throw new ValidationException(parameterName, "Invalid email format.");
        
        return value;
    }
}

