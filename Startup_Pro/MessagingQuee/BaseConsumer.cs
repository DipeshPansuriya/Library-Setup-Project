//-----------------------------------------------------------------------
// <copyright file="BaseConsumer.cs" company="">
//     Author:  
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using MassTransit;
using Microsoft.Extensions.Logging;

namespace KLSPL.Community.Common.Infrastructure.Startup_Proj.MessagingQuee
{
    public class BaseConsumer : IConsumer<BaseMessage>
    {
        readonly ILogger<BaseConsumer> _logger;

        public BaseConsumer(ILogger<BaseConsumer> logger)
        {
            _logger = logger;
        }

        async Task ProcessMessage(BaseMessage message)
        {
            if(!string.IsNullOrEmpty(message.Text))
            {
                Console.WriteLine($"ProcessMessage :- {message}");
                await Task.Delay(100);
            }
            else
            {
                _logger.LogWarning($"Received empty message text, MessageId = {message.MessageId}");
            }
            await Task.CompletedTask;
        }

        public async Task Consume(ConsumeContext<BaseMessage> context)
        {
            string json = GenericFunction.ClassToJson<BaseMessage>(context.Message);
            _logger.LogInformation($"Received message: {json}");

            try
            {
                await ProcessMessage(context.Message);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, ConsoleMessages.ErrorProcessing);
                throw;
            }
        }
    }
}