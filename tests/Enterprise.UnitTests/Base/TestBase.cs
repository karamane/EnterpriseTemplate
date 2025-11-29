using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;

namespace Enterprise.UnitTests.Base;

/// <summary>
/// Tüm unit testlerin base sınıfı
/// AutoFixture ve Moq altyapısı sağlar
/// </summary>
public abstract class TestBase
{
    protected readonly IFixture Fixture;

    protected TestBase()
    {
        Fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });

        // Circular reference'ları önle
        Fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
            .ToList()
            .ForEach(b => Fixture.Behaviors.Remove(b));
        Fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    /// <summary>
    /// Mock oluşturur
    /// </summary>
    protected Mock<T> CreateMock<T>() where T : class
    {
        return Fixture.Freeze<Mock<T>>();
    }

    /// <summary>
    /// Test verisi oluşturur
    /// </summary>
    protected T Create<T>()
    {
        return Fixture.Create<T>();
    }

    /// <summary>
    /// Birden fazla test verisi oluşturur
    /// </summary>
    protected IEnumerable<T> CreateMany<T>(int count = 3)
    {
        return Fixture.CreateMany<T>(count);
    }
}

