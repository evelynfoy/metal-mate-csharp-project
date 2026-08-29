using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Metal_Mate_MVC.Services;
using Metal_Mate_MVC.Models;

namespace AlertGenerator;

public class AlertGenerator
{
    private readonly ILogger _logger;
    private readonly IAlertGeneratorService _alertGeneratorService;

    public AlertGenerator(
        ILoggerFactory loggerFactory,
        IAlertGeneratorService alertGeneratorService)
    {
        _logger = loggerFactory.CreateLogger<AlertGenerator>();
        _alertGeneratorService = alertGeneratorService;
    }

    [Function("GenerateAlerts")]
    public async Task Run([TimerTrigger("0 */30 * * * *")] TimerInfo myTimer)
    {
        _logger.LogInformation(
            "Alert Generator started at: {executionTime}", 
            DateTime.UtcNow);

        try
        {
            await _alertGeneratorService.Run();

            _logger.LogInformation(
            "Alert Generator completed at {ExecutionTime}",
            DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(
            ex,
            "Alert Generator failed at {ExecutionTime}",
            DateTime.UtcNow);
            throw;
        }
    }
}