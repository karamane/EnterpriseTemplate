# Enterprise .NET 10 Application - Unit Test Kılavuzu

**Versiyon:** 1.1  
**Tarih:** Kasım 2025  
**Framework:** .NET 10, C# 14

---

## İçindekiler

1. [Genel Bakış](#1-genel-bakış)
2. [Test Projesi Yapısı](#2-test-projesi-yapısı)
3. [Kullanılan Kütüphaneler](#3-kullanılan-kütüphaneler)
4. [Test Yazma Kuralları](#4-test-yazma-kuralları)
5. [Mock Kullanımı](#5-mock-kullanımı)
6. [Test Örnekleri](#6-test-örnekleri)
7. [Integration Test](#7-integration-test)
8. [Test Çalıştırma](#8-test-çalıştırma)
9. [Code Coverage](#9-code-coverage)

---

## 1. Genel Bakış

### 1.1 Test Piramidi

```
         ┌──────────┐
         │   E2E    │  ← Az sayıda, yavaş
         ├──────────┤
         │Integration│  ← Orta sayıda
         ├──────────┤
         │   Unit   │  ← Çok sayıda, hızlı
         └──────────┘
```

### 1.2 Test Prensipleri

- **FIRST**: Fast, Independent, Repeatable, Self-validating, Timely
- **AAA Pattern**: Arrange, Act, Assert
- **One assertion per test** (mümkün olduğunca)
- **Test isolation**: Her test bağımsız çalışmalı

---

## 2. Test Projesi Yapısı

```
tests/
├── Enterprise.UnitTests/
│   ├── Business/
│   │   ├── Handlers/
│   │   │   └── CreateCustomerHandlerTests.cs
│   │   └── Validators/
│   │       └── CreateCustomerValidatorTests.cs
│   ├── Infrastructure/
│   │   ├── Logging/
│   │   │   └── LogServiceTests.cs
│   │   └── Caching/
│   │       └── RedisCacheServiceTests.cs
│   ├── Api/
│   │   └── Controllers/
│   │       └── CustomersControllerTests.cs
│   ├── Fixtures/
│   │   ├── TestFixtures.cs
│   │   └── AutoFixtureCustomizations.cs
│   └── Mocks/
│       └── MockRepositoryFactory.cs
│
└── Enterprise.IntegrationTests/
    ├── Base/
    │   └── IntegrationTestBase.cs
    ├── Api/
    │   └── CustomerApiTests.cs
    └── Database/
        └── RepositoryTests.cs
```

---

## 3. Kullanılan Kütüphaneler

| Kütüphane | Kullanım |
|-----------|----------|
| **xUnit** | Test framework |
| **Moq** | Mocking |
| **AutoFixture** | Test data generation |
| **FluentAssertions** | Assertion library |
| **Bogus** | Fake data generation |
| **WireMock.Net** | HTTP mock server |
| **Testcontainers** | Docker-based integration tests |

### 3.1 Kurulum

```bash
# Test projesi oluştur
dotnet new xunit -n Enterprise.UnitTests

# Paketleri ekle
dotnet add package Moq
dotnet add package AutoFixture
dotnet add package AutoFixture.Xunit2
dotnet add package AutoFixture.AutoMoq
dotnet add package FluentAssertions
dotnet add package Bogus
```

---

## 4. Test Yazma Kuralları

### 4.1 İsimlendirme Konvansiyonu

```csharp
// Format: [MethodName]_[Scenario]_[ExpectedResult]
public class CustomerServiceTests
{
    [Fact]
    public async Task GetById_WhenCustomerExists_ReturnsCustomer()
    
    [Fact]
    public async Task GetById_WhenCustomerNotFound_ThrowsNotFoundException()
    
    [Fact]
    public async Task Create_WithValidData_ReturnsCreatedCustomer()
    
    [Fact]
    public async Task Create_WithDuplicateEmail_ThrowsBusinessException()
}
```

### 4.2 AAA Pattern

```csharp
[Fact]
public async Task GetById_WhenCustomerExists_ReturnsCustomer()
{
    // Arrange - Hazırlık
    var customerId = Guid.NewGuid();
    var expectedCustomer = new Customer { Id = customerId, Name = "Test" };
    
    _mockRepository
        .Setup(x => x.GetByIdAsync(customerId))
        .ReturnsAsync(expectedCustomer);
    
    // Act - İşlem
    var result = await _sut.GetByIdAsync(customerId);
    
    // Assert - Doğrulama
    result.Should().NotBeNull();
    result.Id.Should().Be(customerId);
    result.Name.Should().Be("Test");
}
```

### 4.3 Test Class Yapısı

```csharp
public class CustomerServiceTests : IDisposable
{
    // System Under Test
    private readonly CustomerService _sut;
    
    // Dependencies (Mocks)
    private readonly Mock<ICustomerRepository> _mockRepository;
    private readonly Mock<ILogger<CustomerService>> _mockLogger;
    private readonly Mock<ICacheService> _mockCache;
    
    // Fixtures
    private readonly IFixture _fixture;
    
    public CustomerServiceTests()
    {
        // Setup mocks
        _mockRepository = new Mock<ICustomerRepository>();
        _mockLogger = new Mock<ILogger<CustomerService>>();
        _mockCache = new Mock<ICacheService>();
        
        // AutoFixture
        _fixture = new Fixture()
            .Customize(new AutoMoqCustomization());
        
        // Create SUT
        _sut = new CustomerService(
            _mockRepository.Object,
            _mockLogger.Object,
            _mockCache.Object);
    }
    
    public void Dispose()
    {
        // Cleanup if needed
    }
}
```

---

## 5. Mock Kullanımı

### 5.1 Basit Mock

```csharp
// Setup
var mockRepo = new Mock<ICustomerRepository>();
mockRepo
    .Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
    .ReturnsAsync(new Customer { Name = "Test" });

// Verify
mockRepo.Verify(x => x.GetByIdAsync(It.IsAny<Guid>()), Times.Once);
```

### 5.2 Sequence Mock

```csharp
// İlk çağrı null, ikinci çağrı customer döner
mockRepo
    .SetupSequence(x => x.GetByIdAsync(It.IsAny<Guid>()))
    .ReturnsAsync((Customer?)null)
    .ReturnsAsync(new Customer { Name = "Test" });
```

### 5.3 Callback ile Mock

```csharp
Customer? savedCustomer = null;

mockRepo
    .Setup(x => x.AddAsync(It.IsAny<Customer>()))
    .Callback<Customer>(c => savedCustomer = c)
    .ReturnsAsync((Customer c) => c);

// Test sonrası
savedCustomer.Should().NotBeNull();
savedCustomer!.Name.Should().Be("Test");
```

### 5.4 Exception Mock

```csharp
mockRepo
    .Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
    .ThrowsAsync(new NotFoundException("Customer not found"));
```

### 5.5 Mock Factory

```csharp
public static class MockRepositoryFactory
{
    public static Mock<ICustomerRepository> CreateCustomerRepository()
    {
        var mock = new Mock<ICustomerRepository>();
        
        // Default setups
        mock.Setup(x => x.ExistsAsync(It.IsAny<Guid>()))
            .ReturnsAsync(true);
            
        return mock;
    }
    
    public static Mock<ICustomerRepository> WithCustomer(
        this Mock<ICustomerRepository> mock, 
        Customer customer)
    {
        mock.Setup(x => x.GetByIdAsync(customer.Id))
            .ReturnsAsync(customer);
        return mock;
    }
}

// Kullanım
var mockRepo = MockRepositoryFactory
    .CreateCustomerRepository()
    .WithCustomer(testCustomer);
```

---

## 6. Test Örnekleri

### 6.1 Service Test

```csharp
public class CustomerServiceTests
{
    private readonly Mock<ICustomerRepository> _mockRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly CustomerService _sut;
    
    public CustomerServiceTests()
    {
        _mockRepository = new Mock<ICustomerRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        
        _sut = new CustomerService(
            _mockRepository.Object,
            _mockUnitOfWork.Object);
    }
    
    [Fact]
    public async Task CreateCustomer_WithValidData_ReturnsCreatedCustomer()
    {
        // Arrange
        var request = new CreateCustomerRequest("John", "Doe", "john@example.com");
        
        _mockRepository
            .Setup(x => x.ExistsByEmailAsync(request.Email))
            .ReturnsAsync(false);
            
        _mockRepository
            .Setup(x => x.AddAsync(It.IsAny<Customer>()))
            .ReturnsAsync((Customer c) => c);
        
        // Act
        var result = await _sut.CreateAsync(request);
        
        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be("John");
        result.Email.Should().Be("john@example.com");
        
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }
    
    [Fact]
    public async Task CreateCustomer_WithDuplicateEmail_ThrowsBusinessException()
    {
        // Arrange
        var request = new CreateCustomerRequest("John", "Doe", "existing@example.com");
        
        _mockRepository
            .Setup(x => x.ExistsByEmailAsync(request.Email))
            .ReturnsAsync(true);
        
        // Act & Assert
        var act = () => _sut.CreateAsync(request);
        
        await act.Should()
            .ThrowAsync<BusinessException>()
            .Where(e => e.ErrorCode.Code == "CUST-002");
    }
}
```

### 6.2 MediatR Handler Test

```csharp
public class CreateCustomerHandlerTests
{
    private readonly Mock<ICustomerRepository> _mockRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IMapper> _mockMapper;
    private readonly CreateCustomerHandler _sut;
    
    public CreateCustomerHandlerTests()
    {
        _mockRepository = new Mock<ICustomerRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockMapper = new Mock<IMapper>();
        
        _sut = new CreateCustomerHandler(
            _mockRepository.Object,
            _mockUnitOfWork.Object,
            _mockMapper.Object);
    }
    
    [Fact]
    public async Task Handle_WithValidCommand_ReturnsCustomerDto()
    {
        // Arrange
        var command = new CreateCustomerCommand("John", "Doe", "john@example.com");
        var customer = new Customer { Id = Guid.NewGuid(), FirstName = "John" };
        var expectedDto = new CustomerDto(customer.Id, "John", "Doe", "john@example.com");
        
        _mockRepository
            .Setup(x => x.ExistsByEmailAsync(command.Email))
            .ReturnsAsync(false);
            
        _mockRepository
            .Setup(x => x.AddAsync(It.IsAny<Customer>()))
            .ReturnsAsync(customer);
            
        _mockMapper
            .Setup(x => x.Map<CustomerDto>(It.IsAny<Customer>()))
            .Returns(expectedDto);
        
        // Act
        var result = await _sut.Handle(command, CancellationToken.None);
        
        // Assert
        result.Should().BeEquivalentTo(expectedDto);
    }
}
```

### 6.3 Validator Test

```csharp
public class CreateCustomerCommandValidatorTests
{
    private readonly CreateCustomerCommandValidator _validator;
    
    public CreateCustomerCommandValidatorTests()
    {
        _validator = new CreateCustomerCommandValidator();
    }
    
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public async Task Validate_WithEmptyFirstName_ReturnsError(string? firstName)
    {
        // Arrange
        var command = new CreateCustomerCommand(firstName!, "Doe", "john@example.com");
        
        // Act
        var result = await _validator.ValidateAsync(command);
        
        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FirstName");
    }
    
    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("john@")]
    public async Task Validate_WithInvalidEmail_ReturnsError(string email)
    {
        // Arrange
        var command = new CreateCustomerCommand("John", "Doe", email);
        
        // Act
        var result = await _validator.ValidateAsync(command);
        
        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }
    
    [Fact]
    public async Task Validate_WithValidData_ReturnsValid()
    {
        // Arrange
        var command = new CreateCustomerCommand("John", "Doe", "john@example.com");
        
        // Act
        var result = await _validator.ValidateAsync(command);
        
        // Assert
        result.IsValid.Should().BeTrue();
    }
}
```

### 6.4 Controller Test

```csharp
public class CustomersControllerTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ICorrelationContext> _mockCorrelationContext;
    private readonly CustomersController _sut;
    
    public CustomersControllerTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockCorrelationContext = new Mock<ICorrelationContext>();
        
        _mockCorrelationContext
            .Setup(x => x.CorrelationId)
            .Returns(Guid.NewGuid().ToString());
        
        _sut = new CustomersController(_mockMediator.Object, _mockCorrelationContext.Object);
    }
    
    [Fact]
    public async Task GetById_WhenCustomerExists_ReturnsOk()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customerDto = new CustomerDto(customerId, "John", "Doe", "john@example.com");
        
        _mockMediator
            .Setup(x => x.Send(It.IsAny<GetCustomerByIdQuery>(), default))
            .ReturnsAsync(customerDto);
        
        // Act
        var result = await _sut.GetById(customerId);
        
        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<CustomerDto>>().Subject;
        response.Data.Should().BeEquivalentTo(customerDto);
    }
    
    [Fact]
    public async Task GetById_WhenCustomerNotFound_ReturnsNotFound()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        
        _mockMediator
            .Setup(x => x.Send(It.IsAny<GetCustomerByIdQuery>(), default))
            .ReturnsAsync((CustomerDto?)null);
        
        // Act
        var result = await _sut.GetById(customerId);
        
        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }
}
```

### 6.5 AutoFixture ile Test

```csharp
public class CustomerServiceAutoFixtureTests
{
    private readonly IFixture _fixture;
    
    public CustomerServiceAutoFixtureTests()
    {
        _fixture = new Fixture()
            .Customize(new AutoMoqCustomization { ConfigureMembers = true });
        
        // Custom conventions
        _fixture.Customize<Customer>(c => c
            .With(x => x.Email, () => $"{Guid.NewGuid():N}@example.com")
            .Without(x => x.Orders));
    }
    
    [Theory, AutoData]
    public async Task GetById_WithAutoGeneratedId_ReturnsCustomer(Guid customerId)
    {
        // Arrange
        var customer = _fixture.Build<Customer>()
            .With(x => x.Id, customerId)
            .Create();
            
        var mockRepo = _fixture.Freeze<Mock<ICustomerRepository>>();
        mockRepo.Setup(x => x.GetByIdAsync(customerId))
            .ReturnsAsync(customer);
        
        var sut = _fixture.Create<CustomerService>();
        
        // Act
        var result = await sut.GetByIdAsync(customerId);
        
        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(customerId);
    }
}
```

---

## 7. Integration Test

### 7.1 WebApplicationFactory

```csharp
public class IntegrationTestBase : IClassFixture<WebApplicationFactory<Program>>
{
    protected readonly HttpClient Client;
    protected readonly WebApplicationFactory<Program> Factory;
    
    public IntegrationTestBase(WebApplicationFactory<Program> factory)
    {
        Factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Test database kullan
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                    
                if (descriptor != null)
                    services.Remove(descriptor);
                
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb");
                });
            });
        });
        
        Client = Factory.CreateClient();
    }
}
```

### 7.2 API Integration Test

```csharp
public class CustomerApiTests : IntegrationTestBase
{
    public CustomerApiTests(WebApplicationFactory<Program> factory) : base(factory) { }
    
    [Fact]
    public async Task CreateCustomer_WithValidData_ReturnsCreated()
    {
        // Arrange
        var request = new
        {
            FirstName = "John",
            LastName = "Doe",
            Email = $"john{Guid.NewGuid():N}@example.com"
        };
        
        // Act
        var response = await Client.PostAsJsonAsync("/api/customers", request);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var content = await response.Content.ReadFromJsonAsync<ApiResponse<CustomerDto>>();
        content!.Data!.FirstName.Should().Be("John");
    }
    
    [Fact]
    public async Task GetCustomer_WhenExists_ReturnsOk()
    {
        // Arrange - Önce customer oluştur
        var createRequest = new
        {
            FirstName = "Jane",
            LastName = "Doe",
            Email = $"jane{Guid.NewGuid():N}@example.com"
        };
        
        var createResponse = await Client.PostAsJsonAsync("/api/customers", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<ApiResponse<CustomerDto>>();
        
        // Act
        var response = await Client.GetAsync($"/api/customers/{created!.Data!.Id}");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
```

### 7.3 Testcontainers ile Database Test

```csharp
public class DatabaseIntegrationTests : IAsyncLifetime
{
    private readonly MsSqlContainer _sqlContainer;
    private ApplicationDbContext _context = null!;
    
    public DatabaseIntegrationTests()
    {
        _sqlContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .Build();
    }
    
    public async Task InitializeAsync()
    {
        await _sqlContainer.StartAsync();
        
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(_sqlContainer.GetConnectionString())
            .Options;
            
        _context = new ApplicationDbContext(options);
        await _context.Database.MigrateAsync();
    }
    
    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _sqlContainer.DisposeAsync();
    }
    
    [Fact]
    public async Task CustomerRepository_AddAndRetrieve_Works()
    {
        // Arrange
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "Customer",
            Email = "test@example.com"
        };
        
        // Act
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();
        
        var retrieved = await _context.Customers.FindAsync(customer.Id);
        
        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Email.Should().Be("test@example.com");
    }
}
```

---

## 8. Test Çalıştırma

### 8.1 Komut Satırı

```bash
# Tüm testleri çalıştır
dotnet test

# Belirli projeyi çalıştır
dotnet test tests/Enterprise.UnitTests/Enterprise.UnitTests.csproj

# Verbose output
dotnet test --logger "console;verbosity=detailed"

# Filtre ile çalıştır
dotnet test --filter "FullyQualifiedName~CustomerService"
dotnet test --filter "Category=Unit"

# Paralel çalıştırma
dotnet test --parallel
```

### 8.2 Test Kategorileri

```csharp
[Trait("Category", "Unit")]
public class CustomerServiceTests { }

[Trait("Category", "Integration")]
public class CustomerApiTests { }

// Çalıştırma
dotnet test --filter "Category=Unit"
```

---

## 9. Code Coverage

### 9.1 Coverage Raporu

```bash
# Coverage ile test çalıştır
dotnet test --collect:"XPlat Code Coverage"

# ReportGenerator ile HTML rapor
dotnet tool install -g dotnet-reportgenerator-globaltool

reportgenerator \
  -reports:"**/coverage.cobertura.xml" \
  -targetdir:"coveragereport" \
  -reporttypes:Html
```

### 9.2 Coverage Hedefleri

| Katman | Minimum Coverage |
|--------|-----------------|
| Business (Handlers) | 90% |
| Validators | 95% |
| Services | 85% |
| Controllers | 80% |
| Infrastructure | 70% |

### 9.3 Coverage Exclude

```xml
<!-- .csproj -->
<PropertyGroup>
  <ExcludeFromCodeCoverage>true</ExcludeFromCodeCoverage>
</PropertyGroup>

<!-- Attribute ile -->
[ExcludeFromCodeCoverage]
public class Migrations { }
```

---

## 10. Best Practices

### 10.1 Do's ✅

- Her public method için en az bir test yaz
- Edge case'leri test et (null, empty, boundary values)
- Test adlarını açıklayıcı yaz
- Arrange-Act-Assert pattern kullan
- Mock'ları Verify et

### 10.2 Don'ts ❌

- Private metodları doğrudan test etme
- Test içinde birden fazla şeyi test etme
- Test'ler arasında paylaşılan state kullanma
- Sleep/Delay kullanma
- External servisleri doğrudan çağırma

---

## Örnek Test Projesi Yapısı

```
tests/Enterprise.UnitTests/
├── Enterprise.UnitTests.csproj
├── GlobalUsings.cs
├── Fixtures/
│   └── TestFixtures.cs
├── Business/
│   └── Handlers/
│       └── CreateCustomerHandlerTests.cs
└── Infrastructure/
    └── Services/
        └── CacheServiceTests.cs
```

### GlobalUsings.cs

```csharp
global using Xunit;
global using Moq;
global using FluentAssertions;
global using AutoFixture;
global using AutoFixture.Xunit2;
global using AutoFixture.AutoMoq;
```

