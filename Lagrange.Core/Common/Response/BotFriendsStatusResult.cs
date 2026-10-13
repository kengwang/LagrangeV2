namespace Lagrange.Core.Common.Response;

/// <summary>The status entries returned by a single QQ friends-status request.</summary>
public sealed record BotFriendsStatusResult(string SelfUid, IReadOnlyList<BotFriendStatus> Friends);

/// <summary>A server-returned UID, unmodified status code, display text and uninterpreted fields.</summary>
public sealed record BotFriendStatus(string Uid, uint? Status, string StatusText, IReadOnlyList<BotFriendStatusRawField> RawFields);

/// <summary>A protobuf field value. Varints use decimal text; other supported wire types use uppercase hex bytes.</summary>
public sealed record BotFriendStatusRawField(uint Field, byte WireType, string Value);
