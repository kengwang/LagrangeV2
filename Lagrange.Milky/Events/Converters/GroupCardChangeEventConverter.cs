using Lagrange.Core.Events.EventArgs;
using Lagrange.Milky.Events.Attributes;
using System.Text.Json.Serialization;
namespace Lagrange.Milky.Events.Converters;
[EventConverter]
public sealed class GroupCardChangeEventConverter : IEventConverter<BotGroupCardChangeEvent, GroupCardChangeEventConverter.Data>
{
    public string Name => "group_member_card_change";
    public bool CanConvert(BotGroupCardChangeEvent e) => true;
    public ValueTask<Data> ConvertAsync(BotGroupCardChangeEvent e, CancellationToken ct) => ValueTask.FromResult(new Data(e.GroupUin, e.UserUin, e.OldCard, e.NewCard));
    public sealed record Data([property: JsonPropertyName("group_id")] long GroupId, [property: JsonPropertyName("user_id")] long UserId,
        [property: JsonPropertyName("old_card")] string OldCard, [property: JsonPropertyName("new_card")] string NewCard);
}
