namespace Lagrange.Core.Common.Response;
public sealed class BotGroupAdminSettingsResult
{
    public uint AddType { get; init; }
    public string GroupQuestion { get; init; } = string.Empty;
    public string GroupAnswer { get; init; } = string.Empty;
    public string MemberInvitePolicy { get; init; } = string.Empty;
    public bool NewMemberHistoryVisible { get; init; }
    public uint RobotMemberSwitch { get; init; }
    public uint RobotMemberExamine { get; init; }
    public uint NoFingerOpen { get; init; }
    public uint NoCodeFingerOpen { get; init; }
    public uint PrivilegeFlag { get; init; }
}
