namespace Lagrange.Core.Common.Response;

public sealed record BotGroupCategoryTag(uint Id, string Name, uint Type);
public sealed record BotGroupAdminPermissions(long GroupId, long UserId, bool? RecallMemberMessage, bool? KickMember, uint? KickMemberPerDay, bool? ReceiveVerify);
