using System;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace OrderProcessingFunctions
{
    public class ProcessingFunction
    {
        private readonly ILogger<ProcessingFunction> _logger;

        public ProcessingFunction(ILogger<ProcessingFunction> logger)
        {
            _logger = logger;
        }

        [Function(nameof(ProcessingFunction))]
        public void Run([ServiceBusTrigger("processing", Connection = "AzureServiceBusConnectionString")] ServiceBusReceivedMessage message)
        {
            _logger.LogInformation("Message ID: {id}", message.MessageId);
            _logger.LogInformation("Message Body: {body}", message.Body);
            _logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);
        }
    }
}
