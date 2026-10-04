using System.Text.Json.Serialization;
namespace Lagrange.Milky.Models.Messages;
public sealed class TempIncomingMessage : IncomingMessageBase
{
    [JsonPropertyName("group_id")] public required long GroupId { get; init; }
}
