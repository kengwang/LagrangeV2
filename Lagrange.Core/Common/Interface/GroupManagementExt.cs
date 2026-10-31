using Lagrange.Core.Common.Response;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Utility;

namespace Lagrange.Core.Common.Interface;

public static class GroupManagementExt
{
    public static async Task<IReadOnlyList<BotGroupCategoryTag>> GetGroupTags(this BotContext context, uint groupClass, CancellationToken ct = default) =>
        (await context.EventContext.SendEvent<GroupTagsResult>(new GetGroupTagsEvent(groupClass), ct)).Tags;

    public static async Task AddGroupTag(this BotContext context, uint groupClass, string name, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        await context.EventContext.SendEvent<GroupManagementSuccess>(new AddGroupTagEvent(groupClass, name), ct);
    }

    public static async Task DeleteGroupTag(this BotContext context, uint groupClass, string name, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        await context.EventContext.SendEvent<GroupManagementSuccess>(new DeleteGroupTagEvent(groupClass, name), ct);
    }

    public static async Task<string> GetGroupShareLink(this BotContext context, long groupId, bool needShortUrl = true, string additionalParam = "", CancellationToken ct = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(groupId);
        ArgumentNullException.ThrowIfNull(additionalParam);
        return (await context.EventContext.SendEvent<GroupShareLinkResult>(new GetGroupShareLinkEvent(groupId, needShortUrl, additionalParam), ct)).Url;
    }

    public static async Task<BotGroupAdminPermissions> GetGroupAdminPermissions(this BotContext context, long groupId, long userId, CancellationToken ct = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(groupId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(userId);
        var permissions = await context.EventContext.SendEvent<AdminPermissionsResult>(new GetAdminPermissionsEvent(groupId, userId), ct);
        var result = ReadPermissions(permissions, groupId, userId);
        var member = await context.CacheContext.ResolveMember(groupId, userId).WaitAsync(ct)
            ?? throw new InvalidDataException("Administrator identity is unavailable.");
        ArgumentException.ThrowIfNullOrWhiteSpace(member.Item2.Uid);
        var verification = await context.EventContext.SendEvent<AdminVerifyResultEvent>(new GetAdminVerifyEvent(groupId, member.Item2.Uid), ct);
        if (verification.GroupId != groupId) throw new InvalidDataException("Administrator verification response has a mismatched group.");
        var matches = verification.Members.Where(x => x.Identity.Uid == member.Item2.Uid).ToArray();
        if (matches.Length != 1 || matches[0].Identity.Uin is { } uin && uin != userId)
            throw new InvalidDataException("Administrator verification response has a mismatched member.");
        return result with { ReceiveVerify = matches[0].MemberFlagExt is { } flags ? (flags & 64) == 0 : null };
    }

    internal static BotGroupAdminPermissions ReadPermissions(AdminPermissionsResult response, long groupId, long userId)
    {
        if (response.GroupId != groupId) throw new InvalidDataException("Administrator permissions response has a mismatched group.");
        var members = response.Members.Where(x => x.MemberUin == userId).ToArray();
        if (members.Length != 1) throw new InvalidDataException("Administrator permissions response must contain exactly one matching member.");
        bool? recall = null, kick = null;
        uint? limit = null;
        var seen = new HashSet<uint>();
        foreach (var item in members[0].Ext?.Permissions?.Items ?? [])
        {
            if (item.Item is not (1 or 2)) continue;
            if (!seen.Add(item.Item) || item.Value > 2) throw new InvalidDataException("Invalid or duplicate administrator permission.");
            bool? enabled = item.Value == 0 ? null : item.Value == 1;
            if (item.Item == 2) recall = enabled;
            else
            {
                kick = enabled;
                if (kick == true) limit = ProtoHelper.Deserialize<AdminKickLimit>(item.Extra).MaxKickPerDay;
            }
        }
        return new(groupId, userId, recall, kick, limit, null);
    }

    public static async Task SetGroupAdminPermissions(this BotContext context, long groupId, long userId, bool? recallMemberMessage = null,
        bool? kickMember = null, uint? kickMemberPerDay = null, CancellationToken ct = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(groupId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(userId);
        var items = BuildPermissions(recallMemberMessage, kickMember, kickMemberPerDay);
        await context.EventContext.SendEvent<GroupManagementSuccess>(new SetAdminPermissionsEvent(groupId, userId, items), ct);
    }

    internal static List<AdminPermissionItem> BuildPermissions(bool? recall, bool? kick, uint? limit)
    {
        if (recall is null && kick is null) throw new ArgumentException("At least one administrator permission is required.");
        if (kick == true && limit is not (0 or 5 or 10 or 20 or 50 or 100 or 200)) throw new ArgumentException("Enabling kick_member requires a supported kick_member_per_day.");
        if (kick != true && limit is not null) throw new ArgumentException("kick_member_per_day requires kick_member=true.");
        var items = new List<AdminPermissionItem>();
        if (kick is { } enabled) items.Add(new AdminPermissionItem { Item = 1, Value = enabled ? 1u : 2u, Extra = enabled ? ProtoHelper.Serialize(new AdminKickLimit { MaxKickPerDay = limit!.Value }).ToArray() : [] });
        if (recall is { } allowed) items.Add(new AdminPermissionItem { Item = 2, Value = allowed ? 1u : 2u });
        return items;
    }

    public static async Task SetGroupAdminReceiveVerify(this BotContext context, long groupId, long userId, bool enable, CancellationToken ct = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(groupId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(userId);
        var result = await context.EventContext.SendEvent<SetAdminVerifyResultEvent>(new SetAdminVerifyEvent(groupId, userId, enable), ct);
        if (result.GroupId != groupId || !result.SuccessfulUins.Contains(userId)) throw new InvalidDataException("Administrator verification change was not confirmed for the target member.");
    }
}
