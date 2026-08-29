using AlertGenerator;
using Metal_Mate_MVC.Models;
using Metal_Mate_MVC.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace Metal_Mate_MVC.Tests
{
    public class AlertGeneratorTests
    {
        private readonly Mock<ILogger<AlertGeneratorService>> _loggerMock;
        private readonly Mock<IAlertRequestService> _alertRequestServiceMock;

        public AlertGeneratorTests()
        {
            _loggerMock = new Mock<ILogger<AlertGeneratorService>>();
            _alertRequestServiceMock = new Mock<IAlertRequestService>();

        }

        [Fact]
        public async Task AlertGenerator_ValidResponse_RetrievesAlertRequests()
        {

            // Arrange
            var requests = new List<AlertRequest>
            {
                new AlertRequest { Id = 1 },
                new AlertRequest { Id = 2 },
                new AlertRequest { Id = 3 }
            };

            _alertRequestServiceMock
                .Setup(x => x.GetAllAlertRequestsAsync())
                .ReturnsAsync(requests);

            var service = new AlertGeneratorService(
                _alertRequestServiceMock.Object, 
                _loggerMock.Object);

            // Act
            await service.Run();

            // Assert
            _alertRequestServiceMock.Verify(
               x => x.GetAllAlertRequestsAsync(),
               Times.Once);

        }


        [Fact]
        public async Task AlertGenerator_Exception_LogsError()
        {

            _alertRequestServiceMock
                .Setup(x => x.GetAllAlertRequestsAsync())
                .ThrowsAsync(new Exception("Database failure"));

            var service = new AlertGeneratorService(
                _alertRequestServiceMock.Object,
                _loggerMock.Object);

            // Act
            var thrownException = await Assert.ThrowsAsync<Exception>(
                () => service.Run());


            // Assert
            Assert.Equal("Database failure", thrownException.Message);

            _loggerMock.Verify(
                     s => s.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.Once);

        }

    }
}