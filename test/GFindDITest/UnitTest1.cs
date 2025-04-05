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
            Assert.Throws<InvalidOperationException>(() =>
                _mockServiceCollection.Object.RegisterScopedServices<object>(assembly));

            // Assert
            _mockServiceCollection.Verify(x => x.Add(It.IsAny<ServiceDescriptor>()), Times.Never());
        }
    }
}