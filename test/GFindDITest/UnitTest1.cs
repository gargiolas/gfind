using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using GFindDI;
using Microsoft.Extensions.Logging;
using Moq;

namespace UnitTests
{
    public class GFindExtensionsTests
    {
        private readonly Mock<IServiceCollection> _mockServiceCollection;
        private readonly Mock<ILogger> _mockLogger;

        public GFindExtensionsTests()
        {
            _mockServiceCollection = new Mock<IServiceCollection>();
            _mockLogger = new Mock<ILogger>();
        }

        [Fact]
        public void RegisterScopedServices_WithExistingService_ShouldRegisterSuccessfully()
        {
            // Arrange
            Assembly assembly = Assembly.GetExecutingAssembly();

            // Act
            GFindExtensions.RegisterScopedServices<object>(_mockServiceCollection.Object, assembly);

            // Assert
            _mockServiceCollection.Verify(x => x.Add(It.IsAny<ServiceDescriptor>()), Times.AtLeastOnce());
        }

        [Fact]
        public void RegisterTransientServices_WithAssembly_ShouldAddTransientServices()
        {
            // Arrange
            Assembly[] assemblies = { Assembly.GetExecutingAssembly() };

            // Act
            GFindExtensions.RegisterTransientServices<object>(_mockServiceCollection.Object, assemblies);

            // Assert
            _mockServiceCollection.Verify(x => x.Add(It.IsAny<ServiceDescriptor>()), Times.AtLeastOnce());
        }

        [Fact]
        public void RegisterSingletonServices_WithAssembly_ShouldAddSingletonServices()
        {
            // Arrange
            Assembly assembly = Assembly.GetExecutingAssembly();

            // Act
            GFindExtensions.RegisterSingletonServices<object>(_mockServiceCollection.Object, assembly);

            // Assert
            _mockServiceCollection.Verify(x => x.Add(It.Is<ServiceDescriptor>(sd =>
                sd.Lifetime == ServiceLifetime.Singleton)), Times.AtLeastOnce());
        }

      
    }
}