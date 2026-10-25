using Lagrange.Milky.Configurations;
using Lagrange.Milky.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FastEndpoints.Mcp;

namespace Lagrange.Milky.Test;

public sealed class McpConfigurationTests
{
    [TestCase(null, false)]
    [TestCase("false", false)]
    [TestCase("true", true)]
    public void ProductionConfigurationBindsMcpOptIn(string? value, bool enabled)
    {
        var builder = WebApplication.CreateSlimBuilder();
        builder.Configuration.Sources.Clear();
        var values = new Dictionary<string, string?>
        {
            ["Milky:HttpServer:Host"] = "127.0.0.1",
            ["Milky:HttpServer:Port"] = "3000",
            ["Milky:AccessToken"] = "mcp-test-token"
        };
        if (value is not null)
            values["Milky:Api:Mcp:Enabled"] = value;
        builder.Configuration.AddInMemoryCollection(values);
        builder.ConfigureMilky();
        using var provider = builder.Services.BuildServiceProvider();
        var configuration = provider.GetRequiredService<MilkyConfiguration>();
        Assert.That(configuration.Api.Mcp.Enabled, Is.EqualTo(enabled));
        Assert.That(configuration.AccessToken, Is.EqualTo("mcp-test-token"));
        Assert.That(provider.GetService<McpOptions>() is not null, Is.EqualTo(enabled));
    }

    [Test]
    public void ConfigurationConstructorKeepsExistingHttpSettingsAndDefaultsMcpOff()
    {
        var http = new MilkyHttpApiConfiguration(true, "https://example.com");
        var configuration = new MilkyApiConfiguration(http);
        Assert.That(configuration.Http, Is.SameAs(http));
        Assert.That(configuration.Mcp.Enabled, Is.False);
    }
}
