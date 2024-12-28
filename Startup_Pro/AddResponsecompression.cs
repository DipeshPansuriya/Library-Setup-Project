using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using System.IO.Compression;

namespace InfraLib.Startup_Pro
{
    public static class AddResponseCompression
    {
        public static void Builder(WebApplicationBuilder builder)
        {
            _ = builder.Services
                .Configure<ApiBehaviorOptions>(
                    options => options.SuppressModelStateInvalidFilter = true);

            _ = builder.Services
                .Configure<BrotliCompressionProviderOptions>(
                    options => options.Level = CompressionLevel.Fastest);

            _ = builder.Services
                .AddResponseCompression(
                    options =>
                    {
                        options.Providers.Add<BrotliCompressionProvider>();
                        options.MimeTypes = ResponseCompressionDefaults.MimeTypes
                            .Concat(new[] { "image/svg+xml" });
                    });
        }
    }
}