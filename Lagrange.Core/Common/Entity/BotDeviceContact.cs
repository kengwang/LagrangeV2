namespace Lagrange.Core.Common.Entity;

public sealed record BotDeviceContact(long Uin, string Uid, string Name, uint TerminalType)
{
    public static IReadOnlyList<BotDeviceContact> Devices { get; } = Array.AsReadOnly<BotDeviceContact>([
        new(0x7e000001, "u_rK7NMsbv2ZjEGPdCuOiCfw", "我的电脑", 1),
        new(0x7e000002, "u_Wcc5rknRRqRO8y5gxMD6sA", "我的手机", 2),
        new(0x7e000003, "u_l7jpPIZxQo0mzJwoEt-SKw", "我的Pad", 3)
    ]);

    internal BotFriend AsFriend() => new(Uin, Name, Uid, Name, string.Empty, string.Empty, new BotFriendCategory(-1, "我的设备", 3, int.MaxValue));
    internal static BotDeviceContact? Find(long uin) => Devices.FirstOrDefault(x => x.Uin == uin);
    internal static uint SourceTerminal(Protocols protocol) => protocol switch
    {
        Protocols.Windows or Protocols.Linux or Protocols.MacOs => 1,
        Protocols.AndroidPhone => 2,
        Protocols.AndroidPad => 3,
        _ => throw new NotSupportedException("Device chats are unavailable for this login platform.")
    };
}
