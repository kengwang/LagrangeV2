using System.Text.Json;

namespace Lagrange.Testing;

internal static class MilkyLogFixture
{
    public static JsonDocument Read() => JsonDocument.Parse(File.ReadAllText(
        Path.Combine(AppContext.BaseDirectory, "Fixtures", "MilkyErrors20261012.json")));

    public static byte[] Bytes(JsonElement fixture) => Convert.FromHexString(fixture.GetProperty("hex").GetString()!);
}
