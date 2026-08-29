using Metal_Mate_MVC.Services;
using Microsoft.Extensions.Logging;

namespace AlertGenerator
{
    public interface IAlertGeneratorService
    {
        Task Run();
    }

    public class AlertGeneratorService : IAlertGeneratorService
    {
        private readonly IAlertRequestService _alertRequestService;
        private readonly ILogger<AlertGeneratorService> _logger;


        public AlertGeneratorService(
            IAlertRequestService alertRequestService,
            ILogger<AlertGeneratorService> logger)
        {
            _alertRequestService = alertRequestService;
            _logger = logger;
        }

        public async Task Run()
        {
            try
            {
                var alertRequests = await _alertRequestService.GetAllAlertRequestsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                ex,
                "Failed to retrieve alert requests {ExecutionTime}",
                DateTime.UtcNow);
                throw;
            }

        }
    }
}
