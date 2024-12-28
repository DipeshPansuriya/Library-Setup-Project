/*
 * <Your-Product-Name>
 * Copyright (c) <Year-From>-<Year-To> <Your-Company-Name>
 *
 * Please configure this header in your SonarCloud/SonarQube quality profile.
 * You can also set it in SonarLint.xml additional file for SonarLint or standalone NuGet analyzer.
 */

using KLSPL.Community.Common.Infrastructure;
using KLSPL.Community.Common.Infrastructure.Startup_Proj.MessagingQuee;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

public static class AddMessaginQuee
{
    public static void AddMessaginQueeBuilder(WebApplicationBuilder builder)
    {
        _ = builder.Services
            .AddMassTransit(
                x =>
                {
                    _ = x.AddConsumer<BaseConsumer>();

                    switch(AppSettings.messagingTransport.QueeType.ToLower())
                    {
                        case "rabbitmq":
                            x.UsingRabbitMq(
                                (context, rmqCfg) =>
                                {
                                    rmqCfg.Host(
                                        new Uri(AppSettings.messagingTransport.Connectionstring),
                                        rmhostdtl =>
                                        {
                                            rmhostdtl.Username(
                                                AppSettings.messagingTransport.UserName);
                                            rmhostdtl.Password(
                                                AppSettings.messagingTransport.UserPassword);
                                        });
                                    rmqCfg.ConfigureEndpoints(context);
                                });
                            break;

                        case "azureservicesbus":
                            x.UsingAzureServiceBus(
                                (context, azcfg) =>
                                {
                                    azcfg.Host(AppSettings.messagingTransport.Connectionstring);
                                    azcfg.ConfigureEndpoints(context);
                                });
                            break;

                        default:
                            x.UsingInMemory(
                                (context, imCfg) =>
                                {
                                    imCfg.ConfigureEndpoints(context);
                                });
                            break;
                    }
                });

        _ = builder.Services.AddScoped<IBasePublisher, BasePublisher>();
    }
}