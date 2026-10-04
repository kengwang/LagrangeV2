namespace Lagrange.Core.Common.Response;
/// <summary>QQ online state and extended status.</summary>
/// <param name="Status">The decoded primary online status.</param>
/// <param name="ExtendedStatus">The decoded extended status, or zero when absent.</param>
public sealed record BotUserStatus(uint Status, uint ExtendedStatus);
/// <summary>Result of relaying an inline keyboard interaction.</summary>
/// <param name="PromptText">The application's response prompt.</param>
public sealed record BotKeyboardClickResult(string PromptText);
/// <summary>A group todo and its authoritative source message identity.</summary>
/// <param name="Sequence">The source group's message sequence.</param>
/// <param name="Random">The source message's random identifier.</param>
/// <param name="Text">The todo's display text.</param>
/// <param name="CreatedAt">The server-provided creation time.</param>
/// <param name="UpdatedAt">The server-provided update time.</param>
public sealed record BotGroupTodoItem(ulong Sequence, long Random, string Text, ulong CreatedAt, ulong UpdatedAt);
/// <summary>An available synthesized voice.</summary>
/// <param name="Id">The voice identifier used for synthesis.</param>
/// <param name="Name">The display name of the voice.</param>
/// <param name="ExampleUrl">The server-provided voice sample URL.</param>
public sealed record BotAiVoice(string Id, string Name, string ExampleUrl);
/// <summary>A category in the AI voice catalog.</summary>
/// <param name="Name">The category's display name.</param>
/// <param name="Voices">The voices belonging to the category.</param>
public sealed record BotAiVoiceCategory(string Name, IReadOnlyList<BotAiVoice> Voices);
