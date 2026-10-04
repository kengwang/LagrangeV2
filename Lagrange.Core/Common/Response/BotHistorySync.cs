using Lagrange.Core.Message;
namespace Lagrange.Core.Common.Response;
/// <summary>A private conversation identity for read-only history probing.</summary>
public sealed record BotHistoryPrivateTarget(long UserUin, string Uid);
/// <summary>Group read and latest sequence positions.</summary>
public sealed record BotGroupHistoryState(long GroupUin, ulong ReadSequence, ulong LatestSequence);
/// <summary>Private read/latest positions and last message time.</summary>
public sealed record BotPrivateHistoryState(long UserUin, string Uid, ulong ReadSequence, ulong LatestSequence, ulong LastMessageTime);
/// <summary>A read-only snapshot of server history positions.</summary>
public sealed record BotHistorySyncState(IReadOnlyList<BotGroupHistoryState> Groups, IReadOnlyList<BotPrivateHistoryState> PrivateUsers);
/// <summary>A bounded forward page; completion refers to the caller's fixed sequence horizon.</summary>
public sealed record BotHistorySyncPage(IReadOnlyList<BotMessage> Messages, ulong? NextSequence, bool IsComplete);
/// <summary>A server-issued private roam cursor; retain both fields for continuation.</summary>
public sealed record BotHistoryRoamCursor(uint Time, uint Random);
/// <summary>A private roam page and server-issued continuation.</summary>
public sealed record BotHistoryRoamPage(IReadOnlyList<BotMessage> Messages, BotHistoryRoamCursor? NextCursor, bool IsComplete);
