using Metal_Mate_MVC.Models;
using Metal_Mate_MVC.Models.ViewModels;
using Metal_Mate_MVC.Services;
using Moq;

namespace Metal_Mate_MVC.Tests.Unit.Services
{
    public class DropdownOptionsServiceTests
    {
        private readonly Mock<IApiService> _apiServiceMock;
        private readonly DropdownOptionsService _service;

        public DropdownOptionsServiceTests()
        {
            _apiServiceMock = new Mock<IApiService>();

            _service = new DropdownOptionsService(
                _apiServiceMock.Object);
        }

        // Mocked response - happy path - Returns populated model
        [Fact]
        public async Task PopulateAsync_ValidResponse_ReturnsPopulatedModel()
        {

            // Arrange
            var model = new AlertRequestViewModel();

            // Mock the API service to return a valid list of metals
            _apiServiceMock
             .Setup(s => s.GetAPIDataAsync<List<Metal>>("symbols"))
             .ReturnsAsync(new List<Metal>
             {
                 new Metal { Name = "Silver", Symbol = "XAG" },
                 new Metal { Name = "Gold", Symbol = "XAU" },

             });

            // Act
            await _service.PopulateAsync(model);

            // Assert
            Assert.NotEmpty(model.Metals);
            Assert.NotEmpty(model.Currencies);

            Assert.Equal("AUD", model.Currencies.First().Value);
            Assert.Equal("Silver", model.Metals.First().Value);

        }

        // Mocked response - API Exception - Propagates exception to caller
        [Fact]
        public async Task PopulateAsync_WhenApiThrows_PropagatesException()
        {
            // Arrange
            var exception = new Exception("API unavailable");

            var model = new AlertRequestViewModel();

            _apiServiceMock
                .Setup(x => x.GetAPIDataAsync<List<Metal>>("symbols"))
                .ThrowsAsync(exception);

            // Act
            var result = await Assert.ThrowsAsync<Exception>(
                   () => _service.PopulateAsync(model));

            // Assert
            Assert.Equal("API unavailable", result.Message);
        }
    }
}
