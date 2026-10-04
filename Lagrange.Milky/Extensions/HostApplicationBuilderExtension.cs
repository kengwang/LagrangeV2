using System;
using System.IO;
using Lagrange.Core.Common;
using Lagrange.Core.Common.Interface;
using Lagrange.Core.Events.EventArgs;
using Lagrange.Milky.Api;
using Lagrange.Milky.Api.Extensions;
using Lagrange.Milky.Caching;
using Lagrange.Milky.Captcha;
using Lagrange.Milky.Configurations;
using Lagrange.Milky.Converters;
using Lagrange.Milky.Events;
using Lagrange.Milky.Events.Extensions;
using Lagrange.Milky.Logging;
using Lagrange.Milky.Login;
using Lagrange.Milky.Serialization;
using Lagrange.Milky.Signing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Lagrange.Milky.Extensions;

public static class HostApplicationBuilderExtension
{
    public static WebApplicationBuilder ConfigureLagrange(this WebApplicationBuilder builder)
    {
        var configuration = builder.Configuration.GetRequiredSection("Lagrange").Get<LagrangeConfiguration>()
            ?? throw new Exception("Failed to load 'Lagrange' configuration");
        builder.Services.AddSingleton(configuration);

        if (configuration.Protocol.Platform is Platform.AndroidPhone or Platform.AndroidPad)
        {
            builder.Services.TryAddSingleton<BotSignProvider, AndroidSigner>();
        }
        else
        {
            builder.Services.TryAddSingleton<BotSignProvider, HttpSigner>();
        }
        builder.Services.AddSingleton(sp =>
        {
            var environment = sp.GetRequiredService<IHostEnvironment>();

            var config = new BotConfig
            {
                Protocol = (Protocols)configuration.Protocol.Platform,
                LogLevel = LogLevel.Trace,
                AutoReconnect = configuration.Server.AutoReconnect,
                UseIPv6Network = configuration.Server.UseIPv6Network,
                GetOptimumServer = configuration.Server.GetOptimumServer,
                AutoReLogin = configuration.Login.AutoReLogin,
                UseNTLogin = configuration.Login.UseNTLogin,
                SignProvider = sp.GetRequiredService<BotSignProvider>(),
            };

            string ksPath = Path.Combine(environment.ContentRootPath, $"{configuration.Login.Uin}.ks");
            var ks = File.Exists(ksPath)
                ? Serializer.JsonDeserialize<BotKeystore>(File.ReadAllText(ksPath))
                    ?? throw new Exception("Failed to deserialize BotKeystore")
                : BotKeystore.CreateEmpty();

            var protocol = (Protocols)configuration.Protocol.Platform;
            var appInfo = configuration.Protocol.AppInfo
                ?? BotAppInfo.ProtocolToAppInfo[protocol];

            return BotFactory.Create(config, ks, appInfo);
        });

        if (configuration.Login.UseOnlineCaptchResolver)
        {
            builder.Services.AddSingleton<ICaptchaResolver, OnlineCaptchaResolver>();
        }
        else builder.Services.AddSingleton<ICaptchaResolver, ManualCaptchaResolver>();

        builder.Services.AddHostedService<LagrangeLoggingService>();
        builder.Services.AddHostedService<LoginService>();

        return builder;
    }

    public static WebApplicationBuilder ConfigureMilky(this WebApplicationBuilder builder)
    {
        var configuration = builder.Configuration.GetRequiredSection("Milky").Get<MilkyConfiguration>()
            ?? throw new Exception("Failed to load 'Milky' configuration");
        builder.Services.AddSingleton(configuration);
        builder.WebHost.UseUrls($"http://{configuration.HttpServer.Host}:{configuration.HttpServer.Port}");

        builder.Services.AddSingleton<MessageCache>();
        builder.Services.AddHostedService<CacheService>();

        builder.Services.AddSingleton<MilkyConverter>();
        builder.Services.AddSingleton<ResourceConverter>();

        builder.Services.AddEventConverters();
        builder.Services.AddSingleton<MilkyEventHub>();
        builder.Services.AddSingleton<IHostedService>(sp => sp.GetRequiredService<MilkyEventHub>());
        builder.Services.AddHttpClient(nameof(WebHookEventHandler), client => client.Timeout = TimeSpan.FromSeconds(10));
        if (configuration.Event.WebHook?.Enabled == true)
            builder.Services.AddHostedService<WebHookEventHandler>();
        return builder;
    }
}
