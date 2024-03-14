using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using VendingMachineFunctions.Services;

namespace VendingMachineFunctions
{
    public class HousekeepingFunction
    {
        private readonly ILogger _logger;

        public HousekeepingFunction(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<HousekeepingFunction>();
        }

        [Function("HousekeepingFunction")]
        public void Run([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer)
        {
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");


            var tableStorageService = new TableStorageService();



            
            if (myTimer.ScheduleStatus is not null)
            {
                _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
            }
        }
    }
}
