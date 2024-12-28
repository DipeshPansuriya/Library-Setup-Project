//-----------------------------------------------------------------------
// <copyright file="BaseMessage.cs" company="">
//     Author:  
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace KLSPL.Community.Common.Infrastructure.Startup_Proj.MessagingQuee
{
    public class BaseMessage
    {
        public string MessageId { get; set; } = Guid.NewGuid().ToString();

        public string Text { get; set; }

        public DateTime TrigerTime { get; set; }
    }
}