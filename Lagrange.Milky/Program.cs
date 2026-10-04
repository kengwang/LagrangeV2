using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Lagrange.Milky.Extensions;
using FastEndpoints;
using FastEndpoints.OpenApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Lagrange.Milky.Security;
using Lagrange.Milky.Api;
using Lagrange_Milky;
using Lagrange.Milky.Serialization;
using Scalar.AspNetCore;

namespace Lagrange.Milky;

public static class Program
{
    public static async Task Main(string[] args)
    {
        bool isNoStdin = false;
        bool isNoStdout = false;
        foreach (string arg in args)
        {
            switch (arg)
            {
                case "--no-stdin":
                {
                    isNoStdin = true;
                    break;
                }
                case "--no-stdout":
                {
                    isNoStdout = true;
                    break;
                }
            }
        }

        SetConsoleEncoding(isNoStdin, isNoStdout);
        ShowApplicationInfo(isNoStdout);
        CheckConfigurationFile(isNoStdin, isNoStdout);

        var builder = WebApplication.CreateSlimBuilder(args);
        builder.ConfigureLagrange();
        builder.ConfigureMilky();
        builder.Services.AddAuthentication("Milky")
            .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, MilkyAuthenticationHandler>("Milky", _ => { });
        builder.Services.AddAuthorization();
        builder.Services
            .AddFastEndpoints(DiscoveredTypes.All)
            .OpenApiDocument(options =>
            {
                options.DocumentName = "v1";
                options.Title = "Lagrange.Milky API";
                options.Version = "v1";
                options.ExcludeNonFastEndpoints = true;
                options.ShortSchemaNames = true;
                options.EnableJWTBearerAuth = false;
            });

        var host = builder.Build();
        host.UseWebSockets();
        host.UseAuthentication();
        host.UseAuthorization();
        host.UseFastEndpoints(c =>
        {
            c.Serializer.Options.AddSerializerContextsFromLagrange_Milky();
            c.Errors.ResponseBuilder = (failures, context, _) =>
            {
                context.Response.StatusCode = StatusCodes.Status200OK;
                var failure = failures.FirstOrDefault();
                return new MilkyApiResponse(-400, failure?.ErrorMessage ?? "request validation failed");
            };
        });
        host.MapOpenApi();
        host.MapScalarApiReference(options =>
        {
            options.AddDocuments("v1");
            options.OperationTitleSource = OperationTitleSource.Path;
        });

        await host.RunAsync();
    }

    private static void SetConsoleEncoding(bool isNoStdin, bool isNoStdout)
    {
        if (!isNoStdin) Console.InputEncoding = Encoding.UTF8;
        if (!isNoStdout) Console.OutputEncoding = Encoding.UTF8;
    }

    private static void ShowApplicationInfo(bool isNoStdout)
    {
        if (isNoStdout) return;

        string hash = Constants.GitHash;
        Console.WriteLine($"""
        ╭─────────────────────────────────────────╮
        │  _                                      │
        │ | |   __ _ __ _ _ _ __ _ _ _  __ _ ___  │
        │ | |__/ _` / _` | '_/ _` | ' \/ _` / -_) │
        │ |____\__,_\__, |_| \__,_|_||_\__, \___| │
        │           |___/              |___/      │
        │                    __  __ _ _ _         │
        │                   |  \/  (_) | |___  _  │
        │                   | |\/| | | | / / || | │
        ├─────────╮         |_|  |_|_|_|_\_\\_, | │
        │{hash,-9}│                          |__/ │
        ╰─────────┴───────────────────────────────╯
        """);
    }

    private static void CheckConfigurationFile(bool isNoStdin, bool isNoStdout)
    {
        if (File.Exists("appsettings.json")) return;

        if (!isNoStdout)
        {
            Console.WriteLine($"appsettings.json not found. Create a default configuration file...");
        }

        using var input = typeof(Program).Assembly.GetManifestResourceStream("Lagrange.Milky.Resources.appsettings.json");
        if (input == null) throw new Exception("Default configuration file not found");

        using (var output = File.OpenWrite("appsettings.json")) input.CopyTo(output);

        if (!isNoStdout)
        {
            Console.WriteLine("Please edit the configuration file");
            Console.WriteLine("and press any key to continue starting the application.");
            if (!isNoStdin)
            {
                Console.ReadKey();
            }
        }
    }
}
