//-----------------------------------------------------------------------
// <copyright file="GenericCacheInputDto.cs" company="">
//     Author:  
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace KLSPL.Community.Common.Infrastructure.Startup_Proj.Cache
{
    public class GenericCacheInputDto
    {
        public string contextKey { get; set; }

        public DateTimeOffset? expirationTime { get; set; } = null;

        public string objectKey { get; set; }

        public string parentKey { get; set; }

        public object value { get; set; }
    }
}