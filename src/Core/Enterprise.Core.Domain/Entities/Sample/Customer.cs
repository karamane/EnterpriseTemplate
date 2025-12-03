namespace Enterprise.Core.Domain.Entities.Sample;

/// <summary>
/// Örnek Customer entity
/// Geliştiriciler için referans entity
/// Oracle: NUMBER ID, SqlServer: GUID ID için SoftDeleteEntity&lt;long&gt; veya SoftDeleteEntity kullanın
/// </summary>
public class Customer : SoftDeleteEntity<long>
{
    /// <summary>
    /// Müşteri adı
    /// </summary>
    public string FirstName { get; private set; } = string.Empty;

    /// <summary>
    /// Müşteri soyadı
    /// </summary>
    public string LastName { get; private set; } = string.Empty;

    /// <summary>
    /// Email adresi
    /// </summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>
    /// Telefon numarası
    /// </summary>
    public string? PhoneNumber { get; private set; }

    /// <summary>
    /// Müşteri aktif mi?
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Kayıt tarihi
    /// </summary>
    public DateTime RegisteredAt { get; private set; }

    /// <summary>
    /// Constructor (EF Core için)
    /// </summary>
    protected Customer() { }

    /// <summary>
    /// Yeni müşteri oluşturur
    /// ID Oracle sequence tarafından atanır
    /// </summary>
    public static Customer Create(
        string firstName,
        string lastName,
        string email,
        string? phoneNumber = null)
    {
        var customer = new Customer
        {
            // ID Oracle sequence tarafından atanacak (Identity)
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            PhoneNumber = phoneNumber,
            IsActive = true,
            RegisteredAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        return customer;
    }

    /// <summary>
    /// Domain event eklemek için (ID atandıktan sonra çağrılmalı)
    /// </summary>
    public void RaiseCreatedEvent()
    {
        AddDomainEvent(new CustomerCreatedEvent(Id, Email));
    }

    /// <summary>
    /// Müşteri bilgilerini günceller
    /// </summary>
    public void Update(string firstName, string lastName, string? phoneNumber)
    {
        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Email günceller
    /// </summary>
    public void UpdateEmail(string newEmail)
    {
        var oldEmail = Email;
        Email = newEmail;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new CustomerEmailChangedEvent(Id, oldEmail, newEmail));
    }

    /// <summary>
    /// Müşteriyi deaktif eder
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Müşteriyi aktif eder
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Full name
    /// </summary>
    public string FullName => $"{FirstName} {LastName}";
}

#region Domain Events

/// <summary>
/// Müşteri oluşturuldu event'i
/// </summary>
public record CustomerCreatedEvent(long CustomerId, string Email) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

/// <summary>
/// Müşteri email değişti event'i
/// </summary>
public record CustomerEmailChangedEvent(long CustomerId, string OldEmail, string NewEmail) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

#endregion

