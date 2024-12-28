//-----------------------------------------------------------------------
// <copyright file="BasePublisher.cs" company="">
//     Author:  
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using MassTransit;

namespace KLSPL.Community.Common.Infrastructure.Startup_Proj.MessagingQuee
{
    public interface IBasePublisher
    {
        Task PublishSampleMessage(BaseMessage message);
    }

    public class BasePublisher : IBasePublisher
    {
        readonly IPublishEndpoint _publishEndpoint;

        public BasePublisher(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        public async Task PublishSampleMessage(BaseMessage message)
        {
            await _publishEndpoint.Publish(message);
        }
    }
}