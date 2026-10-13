using System.Reflection;
using System.Text.Json;
using Lagrange.Core;
using Lagrange.Core.Common;
using Lagrange.Core.Common.Interface;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Services;
using Lagrange.Core.Events.EventArgs;
using Lagrange.Milky.Captcha;
using Lagrange.Milky.Configurations;
using Lagrange.Milky.Extensions;
using Lagrange.Milky.Login;
using Lagrange.Milky.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog.Debugging;

namespace Lagrange.Milky.Test;

[NonParallelizable]
public sealed class LoggingTests
{
    private string _directory = null!;
    private TextWriter _originalOutput = null!;
    private StringWriter _output = null!;

    [SetUp]
    public void SetUp()
    {
        _directory = Directory.CreateTempSubdirectory("milky-logging-").FullName;
        _originalOutput = Console.Out;
        _output = new StringWriter();
        Console.SetOut(_output);
    }

    [TearDown]
    public void TearDown()
    {
        Console.SetOut(_originalOutput);
        SelfLog.Disable();
        _output.Dispose();
        Directory.Delete(_directory, recursive: true);
    }

    private HostApplicationBuilder Builder(params (string Key, string? Value)[] overrides)
    {
        var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings { ContentRootPath = _directory, Args = [] });
        builder.Configuration.Sources.Clear();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Logging:LogLevel:Default"] = "Information",
            ["Logging:JsonFile:Enabled"] = "true",
        });
        builder.Configuration.AddInMemoryCollection(overrides.ToDictionary(x => x.Key, x => x.Value));
        return builder;
    }

    private string[] Lines() => Directory.GetFiles(_directory, "*.jsonl", SearchOption.AllDirectories)
        .Order(StringComparer.Ordinal).SelectMany(File.ReadAllLines).ToArray();

    [Test]
    public void DefaultConfigurationWritesConsoleOnceWithoutCreatingFiles()
    {
        var builder = Builder();
        builder.Configuration["Logging:JsonFile:Enabled"] = null;
        using var root = builder.ConfigureMilkyLogging();
        using var host = builder.Build();
        host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Test.Default").LogInformation("console-only-marker");
        Assert.That(Directory.GetFiles(_directory, "*", SearchOption.AllDirectories), Is.Empty);
        Assert.That(_output.ToString().Split("console-only-marker").Length - 1, Is.EqualTo(1));
        Assert.That(_output.ToString(), Does.Contain("Test.Default"));
    }

    [Test]
    public void JsonPreservesPropertiesScopesExceptionsAndFlushesAfterHostDisposal()
    {
        var builder = Builder();
        using (var root = builder.ConfigureMilkyLogging())
        {
            using var host = builder.Build();
            var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Test.Structured");
            using (logger.BeginScope(new Dictionary<string, object> { ["RequestId"] = "request-123" }))
                logger.LogError(new InvalidOperationException("failure-marker"), "Failed item {ItemId}: {Text}", 42, "中文\n\"quoted\"");
            logger.LogCritical("shutdown-marker");
        }
        var lines = Lines();
        Assert.That(lines, Has.Length.EqualTo(2));
        using var document = JsonDocument.Parse(lines[0]);
        var entry = document.RootElement;
        Assert.That(entry.GetProperty("Timestamp").GetDateTimeOffset(), Is.Not.EqualTo(default(DateTimeOffset)));
        Assert.That(entry.GetProperty("Level").GetString(), Is.EqualTo("Error"));
        Assert.That(entry.GetProperty("MessageTemplate").GetString(), Is.EqualTo("Failed item {ItemId}: {Text}"));
        Assert.That(entry.GetProperty("RenderedMessage").GetString(), Does.Contain("42"));
        Assert.That(entry.GetProperty("Exception").GetString(), Does.Contain("failure-marker"));
        var properties = entry.GetProperty("Properties");
        Assert.That(properties.GetProperty("ItemId").GetInt32(), Is.EqualTo(42));
        Assert.That(properties.GetProperty("Text").GetString(), Is.EqualTo("中文\n\"quoted\""));
        Assert.That(properties.GetProperty("SourceContext").GetString(), Is.EqualTo("Test.Structured"));
        Assert.That(properties.GetProperty("RequestId").GetString(), Is.EqualTo("request-123"));
        using var final = JsonDocument.Parse(lines[1]);
        Assert.That(final.RootElement.GetProperty("RenderedMessage").GetString(), Is.EqualTo("shutdown-marker"));
        Assert.That(_output.ToString().Split("shutdown-marker").Length - 1, Is.EqualTo(1));
    }

    [Test]
    public void ExistingDefaultAndCategoryLevelFiltersRemainAuthoritative()
    {
        var builder = Builder(("Logging:LogLevel:Default", "Warning"), ("Logging:LogLevel:Test.Verbose", "Trace"),
            ("Logging:LogLevel:Test.Silent", "None"));
        using (var root = builder.ConfigureMilkyLogging())
        {
            using var host = builder.Build();
            var factory = host.Services.GetRequiredService<ILoggerFactory>();
            var normal = factory.CreateLogger("Test.Default");
            normal.LogInformation("filtered-marker");
            normal.LogWarning("warning-marker");
            factory.CreateLogger("Test.Verbose.Child").LogTrace("trace-marker");
            factory.CreateLogger("Test.Silent").LogCritical("silent-marker");
        }
        var entries = string.Join('\n', Lines());
        Assert.That(Lines(), Has.Length.EqualTo(2));
        Assert.That(entries, Does.Contain("warning-marker").And.Contain("trace-marker"));
        Assert.That(entries, Does.Not.Contain("filtered-marker").And.Not.Contain("silent-marker"));
        Assert.That(_output.ToString(), Does.Not.Contain("filtered-marker").And.Not.Contain("silent-marker"));
    }

    [Test]
    public void FileRollsBySizeAndRetainsConfiguredNumberOfFiles()
    {
        var builder = Builder(("Logging:JsonFile:FileSizeLimitBytes", "1"), ("Logging:JsonFile:RetainedFileCountLimit", "2"));
        using (var root = builder.ConfigureMilkyLogging())
        {
            using var host = builder.Build();
            var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Test.Roll");
            for (int i = 0; i < 5; i++) logger.LogInformation("roll-marker {Index}", i);
        }
        Assert.That(Directory.GetFiles(Path.Combine(_directory, "logs"), $"milky-{DateTime.Now:yyyyMMdd}*.jsonl"), Has.Length.EqualTo(2));
        var lines = Lines();
        Assert.That(lines, Has.Length.EqualTo(2));
        using var last = JsonDocument.Parse(lines[^1]);
        Assert.That(last.RootElement.GetProperty("Properties").GetProperty("Index").GetInt32(), Is.EqualTo(4));
    }

    [TestCase("Logging:JsonFile:Path", " ")]
    [TestCase("Logging:JsonFile:RetainedFileCountLimit", "0")]
    [TestCase("Logging:JsonFile:RetainedFileCountLimit", "-1")]
    [TestCase("Logging:JsonFile:FileSizeLimitBytes", "0")]
    [TestCase("Logging:JsonFile:FileSizeLimitBytes", "-1")]
    public void EnabledFileRejectsInvalidConfiguration(string key, string value)
    {
        var builder = Builder((key, value));
        Assert.That(() => builder.ConfigureMilkyLogging(), Throws.InstanceOf<ArgumentException>());
        Assert.That(Directory.GetFiles(_directory, "*", SearchOption.AllDirectories), Is.Empty);
    }

    [Test]
    public void DisabledFileDoesNotValidateUnusedSettings()
    {
        var builder = Builder(("Logging:JsonFile:Enabled", "false"), ("Logging:JsonFile:Path", " "),
            ("Logging:JsonFile:RetainedFileCountLimit", "0"), ("Logging:JsonFile:FileSizeLimitBytes", "0"));
        using var root = builder.ConfigureMilkyLogging();
        using var host = builder.Build();
        Assert.That(Directory.GetFiles(_directory, "*", SearchOption.AllDirectories), Is.Empty);
    }

    [Test]
    public void AbsolutePathIsUsedWithoutContentRootPrefix()
    {
        var builder = Builder(("Logging:JsonFile:Path", Path.Combine(_directory, "absolute-.jsonl")));
        using (var root = builder.ConfigureMilkyLogging())
        {
            using var host = builder.Build();
            host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Test.Absolute").LogInformation("absolute-marker");
        }
        Assert.That(Directory.GetFiles(_directory, "absolute-*.jsonl"), Has.Length.EqualTo(1));
        Assert.That(Directory.Exists(Path.Combine(_directory, "logs")), Is.False);
    }

    [Test]
    public void FileOpenFailureIsDiagnosedAndConsoleLoggingContinues()
    {
        string blocker = Path.Combine(_directory, "blocker");
        File.WriteAllText(blocker, "This is a file, not a directory.");
        using var diagnostics = new StringWriter();
        SelfLog.Enable(TextWriter.Synchronized(diagnostics));
        var builder = Builder(("Logging:JsonFile:Path", Path.Combine(blocker, "milky-.jsonl")));
        using (var root = builder.ConfigureMilkyLogging())
        {
            using var host = builder.Build();
            host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Test.Failure").LogInformation("fallback-marker");
        }
        SelfLog.Disable();
        Assert.That(diagnostics.ToString(), Does.Contain("blocker"));
        Assert.That(_output.ToString(), Does.Contain("fallback-marker"));
    }

    [TestCase("None")]
    [TestCase("Trace")]
    public async Task ScanOutputBypassesLogger(string minimumLevel)
    {
        var builder = Builder(("Logging:LogLevel:Default", minimumLevel));
        using (var root = builder.ConfigureMilkyLogging())
        {
            using var host = builder.Build();
            var settings = new LagrangeConfiguration(new(), new(123));
            using var bot = BotFactory.Create(new BotConfig { GetOptimumServer = false }, new BotKeystore(), BotAppInfo.ProtocolToAppInfo[Protocols.Linux]);
            var resolver = new OnlineCaptchaResolver(host.Services.GetRequiredService<ILogger<OnlineCaptchaResolver>>(), settings);
            var service = new LoginService(host.Services.GetRequiredService<IHostEnvironment>(),
                host.Services.GetRequiredService<ILogger<LoginService>>(), settings, bot, resolver);
            // Exercise the callbacks without a QQ login or an HTTP request.
            using var loginCancellation = new CancellationTokenSource();
            typeof(LoginService).GetField("_cts", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(service, loginCancellation);
            string previousDirectory = Environment.CurrentDirectory;
            try
            {
                Environment.CurrentDirectory = _directory;
                await (Task)typeof(LoginService).GetMethod("OnQrCode", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .Invoke(service, [bot, new BotQrCodeEvent("https://example.test/login", [1, 2, 3])])!;
                Assert.That(File.ReadAllBytes(Path.Combine(_directory, "qrcode.png")), Is.EqualTo(new byte[] { 1, 2, 3 }));
            }
            finally
            {
                Environment.CurrentDirectory = previousDirectory;
            }
            typeof(LoginService).GetMethod("OnNewDeviceVerify", BindingFlags.Instance | BindingFlags.NonPublic)!
                .Invoke(service, [bot, new BotNewDeviceVerifyEvent("https://example.test/device")]);
            using var canceled = new CancellationTokenSource();
            canceled.Cancel();
            Assert.ThrowsAsync<OperationCanceledException>(async () =>
                await resolver.ResolveCaptchaAsync("https://example.test/captcha?uin=0", canceled.Token));
            typeof(Program).GetMethod("ShowApplicationInfo", BindingFlags.Static | BindingFlags.NonPublic)!.Invoke(null, [false]);
        }
        Assert.That(_output.ToString(), Does.Contain("https://example.test/login").And.Contain("https://example.test/device").And.Contain("Please scan the QR code or open"));
        Assert.That(_output.ToString(), Does.Contain("╭─────────────────────────────────────────╮"));
        Assert.That(Lines(), Is.Empty);
    }

    [Test]
    public void CoreLogBridgePreservesCategoryLevelAndException()
    {
        var builder = Builder();
        using (var root = builder.ConfigureMilkyLogging())
        {
            using var host = builder.Build();
            using var bot = BotFactory.Create(new BotConfig { GetOptimumServer = false }, new BotKeystore(), BotAppInfo.ProtocolToAppInfo[Protocols.Linux]);
            var bridge = new LagrangeLoggingService(host.Services.GetRequiredService<ILoggerFactory>(), bot);
            typeof(LagrangeLoggingService).GetMethod("OnLog", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(bridge,
                [bot, new BotLogEvent("Core.Packet", Lagrange.Core.Events.EventArgs.LogLevel.Error, "core-marker", new InvalidOperationException("core-exception"))]);
        }
        using var document = JsonDocument.Parse(Lines().Single());
        var entry = document.RootElement;
        Assert.That(entry.GetProperty("Level").GetString(), Is.EqualTo("Error"));
        Assert.That(entry.GetProperty("RenderedMessage").GetString(), Does.Contain("core-marker"));
        Assert.That(entry.GetProperty("Properties").GetProperty("Message").GetString(), Is.EqualTo("core-marker"));
        Assert.That(entry.GetProperty("Properties").GetProperty("SourceContext").GetString(), Is.EqualTo("Core.Packet"));
        Assert.That(entry.GetProperty("Exception").GetString(), Does.Contain("core-exception"));
    }

    [Test]
    public void PacketTraceBridgeWritesGeneratedTemplatesAndTypedJsonProperties()
    {
        var builder = Builder(("Logging:LogLevel:PacketContext", "Trace"));
        using (var root = builder.ConfigureMilkyLogging())
        {
            using var host = builder.Build();
            using var bot = BotFactory.Create(new BotConfig(), new BotKeystore(), BotAppInfo.ProtocolToAppInfo[Protocols.Linux]);
            var bridge = new LagrangeLoggingService(host.Services.GetRequiredService<ILoggerFactory>(), bot);
            var log = typeof(LagrangeLoggingService).GetMethod("OnLog", BindingFlags.Instance | BindingFlags.NonPublic)!;
            log.Invoke(bridge, [bot, BotPacketLogEvent.Create(new BotSsoPacket("test.command", new byte[] { 8, 1, 0xAB }, 42), new ServiceAttribute("test.command", RequestType.D2Auth, EncryptType.EncryptD2Key))]);
            log.Invoke(bridge, [bot, BotPacketLogEvent.Create(new BotSsoPacket("test.command", ReadOnlyMemory<byte>.Empty, 42, 7, "server \"detail\"\n中文"))]);
        }
        Assert.That(Lines(), Has.Length.EqualTo(2));
        using var sent = JsonDocument.Parse(Lines()[0]);
        using var received = JsonDocument.Parse(Lines()[1]);
        var outgoing = sent.RootElement.GetProperty("Properties");
        var incoming = received.RootElement.GetProperty("Properties");
        Assert.Multiple(() =>
        {
            Assert.That(sent.RootElement.GetProperty("Level").GetString(), Is.EqualTo("Verbose"));
            Assert.That(sent.RootElement.GetProperty("MessageTemplate").GetString(), Does.Contain("{Command}").And.Contain("{Data}"));
            Assert.That(outgoing.GetProperty("Direction").GetString(), Is.EqualTo("Outgoing"));
            Assert.That(outgoing.GetProperty("Sequence").GetInt32(), Is.EqualTo(42));
            Assert.That(outgoing.GetProperty("Command").GetString(), Is.EqualTo("test.command"));
            Assert.That(outgoing.GetProperty("RetCode").GetInt32(), Is.Zero);
            Assert.That(outgoing.GetProperty("RequestType").GetString(), Is.EqualTo("D2Auth"));
            Assert.That(outgoing.GetProperty("EncryptType").GetString(), Is.EqualTo("EncryptD2Key"));
            Assert.That(outgoing.GetProperty("DataLength").GetInt32(), Is.EqualTo(3));
            Assert.That(outgoing.GetProperty("Data").GetString(), Is.EqualTo("0801AB"));
            Assert.That(outgoing.GetProperty("SourceContext").GetString(), Is.EqualTo("PacketContext"));
            Assert.That(outgoing.GetProperty("EventId").GetProperty("Id").GetInt32(), Is.EqualTo(1001));
            Assert.That(outgoing.TryGetProperty("Message", out _), Is.False);
            Assert.That(incoming.GetProperty("Direction").GetString(), Is.EqualTo("Incoming"));
            Assert.That(incoming.GetProperty("RetCode").GetInt32(), Is.EqualTo(7));
            Assert.That(incoming.GetProperty("Extra").GetString(), Is.EqualTo("server \"detail\"\n中文"));
            Assert.That(incoming.GetProperty("DataLength").GetInt32(), Is.Zero);
            Assert.That(incoming.GetProperty("Data").GetString(), Is.Empty);
            Assert.That(incoming.GetProperty("EventId").GetProperty("Id").GetInt32(), Is.EqualTo(1002));
        });
    }

    [Test]
    public void PacketTraceBridgeHonorsCategoryFilter()
    {
        var builder = Builder(("Logging:LogLevel:Default", "Trace"), ("Logging:LogLevel:PacketContext", "Information"));
        using (var root = builder.ConfigureMilkyLogging())
        {
            using var host = builder.Build();
            using var bot = BotFactory.Create(new BotConfig(), new BotKeystore(), BotAppInfo.ProtocolToAppInfo[Protocols.Linux]);
            var bridge = new LagrangeLoggingService(host.Services.GetRequiredService<ILoggerFactory>(), bot);
            typeof(LagrangeLoggingService).GetMethod("OnLog", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(bridge,
                [bot, BotPacketLogEvent.Create(new BotSsoPacket("hidden-packet", new byte[] { 8, 1 }, 42))]);
        }
        Assert.That(Lines(), Is.Empty);
        Assert.That(_output.ToString(), Does.Not.Contain("hidden-packet"));
    }

    [Test]
    public void BundledConfigurationExcludesHeartbeatAlive()
    {
        using var stream = typeof(Program).Assembly.GetManifestResourceStream("Lagrange.Milky.Resources.appsettings.json")!;
        using var document = JsonDocument.Parse(stream, new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true });
        var commands = document.RootElement.GetProperty("Milky").GetProperty("Event").GetProperty("RawPacketCommandBlacklist");
        Assert.That(commands.EnumerateArray().Select(x => x.GetString()), Does.Contain("Heartbeat.Alive"));
    }
}
