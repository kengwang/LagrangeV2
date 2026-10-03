using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core.Events.EventArgs;
using Lagrange.Milky.Events.Attributes;

namespace Lagrange.Milky.Events.Converters;

[EventConverter]
public sealed class GroupAdminEventConverter : IEventConverter<BotGroupAdminEvent, GroupAdminEventConverter.Data>
{
    public string Name => "group_admin_change";
    public bool CanConvert(BotGroupAdminEvent @event) => true;
    public ValueTask<Data> ConvertAsync(BotGroupAdminEvent @event, CancellationToken ct) => ValueTask.FromResult(new Data
    {
        GroupId = @event.GroupUin, UserId = @event.UserUin, IsPromote = @event.IsPromote,
    });

    public sealed class Data
    {
        [JsonPropertyName("group_id")] public required long GroupId { get; init; }
        [JsonPropertyName("user_id")] public required long UserId { get; init; }
        [JsonPropertyName("is_promote")] public required bool IsPromote { get; init; }
    }
}
