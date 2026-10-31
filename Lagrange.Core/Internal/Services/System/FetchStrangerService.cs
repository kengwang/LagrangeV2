using System.Buffers.Binary;
using System.Text;
using Lagrange.Core.Common;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Internal.Packets.Service.Migration;
using Lagrange.Core.Services;
using Lagrange.Core.Utility;

namespace Lagrange.Core.Internal.Services.System;

[EventSubscribe<FetchStrangerByUinEventReq>(Protocols.All)]
[EventSubscribe<FetchStrangerByUidEventReq>(Protocols.All)]
[EventSubscribe<GetUserStatusEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0xfe1_2")]
internal class FetchStrangerService : BaseService<FetchStrangerEventReqBase, FetchStrangerEventResp>
{
    private static readonly uint Command = 0xfe1;

    private static readonly uint Service = 2;

    private static string Tag => $"OidbSvcTrpcTcp.0x{Command:X}_{Service}";

    public static readonly List<FetchStrangerRequestKey> Keys = [
        new() { Key = 101 },      // Avatar; Object
        new() { Key = 102 },      // Sign
        new() { Key = 103 },      // Remark
        new() { Key = 104 },      // Tag; Object
        new() { Key = 105 },      // Level
        new() { Key = 107 },      // Business List; Object
        new() { Key = 20002 },    // Nickname
        new() { Key = 20003 },    // Country
        new() { Key = 20004 },    // City
        new() { Key = 20006 },    // Home City
        new() { Key = 20009 },    // Gender; 1 Male 2 Female 255 Unknown
        new() { Key = 20011 },    // EMail
        new() { Key = 20016 },    // Desensitized mobile phone number
        new() { Key = 20020 },    // Municipal district
        new() { Key = 20021 },    // School
        new() { Key = 20026 },    // Registration Time; Only year, hour, minute, second
        new() { Key = 20031 },    // Birthday; 
        new() { Key = 20037 },    // Age
        new() { Key = 27394 },    // QID
        new() { Key = 41756 },
        new() { Key = 41757 },
        new() { Key = 42241 },
    ];

    protected override ValueTask<ReadOnlyMemory<byte>> Build(FetchStrangerEventReqBase input, BotContext context)
    {
        return ValueTask.FromResult(input switch
        {
            GetUserStatusEventReq req => ProtoHelper.Serialize(new Oidb
            {
                Command = Command,
                Service = Service,
                Body = ProtoHelper.Serialize(req.Body),
                Reserved = 1
            }),
            FetchStrangerByUinEventReq req => ProtoHelper.Serialize(new Oidb
            {
                Command = Command,
                Service = Service,
                Body = ProtoHelper.Serialize(new FetchStrangerByUinRequest
                {
                    Uin = req.Uin,
                    Keys = Keys,
                }),
                Reserved = 1
            }),
            FetchStrangerByUidEventReq req => ProtoHelper.Serialize(new Oidb
            {
                Command = Command,
                Service = Service,
                Body = ProtoHelper.Serialize(new FetchStrangerByUidRequest
                {
                    Uid = req.Uid,
                    Keys = Keys,
                })
            }),
            _ => throw new NotSupportedException(),
        });
    }

    protected override ValueTask<FetchStrangerEventResp> Parse(ReadOnlyMemory<byte> input, BotContext context)
    {
        var oidb = ProtoHelper.Deserialize<Oidb>(input.Span);
        if (oidb.Result != 0)
        {
            context.LogWarning(Tag, "Error: {0}, Message: {1}", null, oidb.Result, oidb.Message);
            throw new OperationException((int)oidb.Result, oidb.Message);
        }
        var response = ProtoHelper.Deserialize<FetchStrangerResponse>(oidb.Body.Span);
        var status = ProtoHelper.Deserialize<OidbStrangerStatusResp>(oidb.Body.Span);
        // A status-only response does not contain profile fields. Decode those only for profile callers.
        return ValueTask.FromResult(new FetchStrangerEventResp(() => ParseStranger(response), status));
    }

    private static BotStranger ParseStranger(FetchStrangerResponse response)
    {
        var numbers = response.Body.Properties.NumberProperties.ToDictionary(
            property => property.Key,
            property => property.Value
        );
        var bytes = response.Body.Properties.BytesProperties.ToDictionary(
            property => property.Key,
            property => property.Value
        );

        // Check exists
        if (!bytes.TryGetValue(20002, out byte[]? nicknameBytes))
        {
            throw new OperationException(-1, "Stranger not found");
        }

        // Birthday
        var (year, month, day) = ReadBirthday(bytes.GetValueOrDefault(20031UL));
        ulong? vipFlags = numbers.TryGetValue(41756, out var flags) ? flags : null;
        uint? vipLevel = null;
        if (numbers.TryGetValue(41757, out var level))
        {
            var mask = level & 0xfff;
            if (mask > 0) vipLevel = (uint)global::System.Numerics.BitOperations.Log2(mask) + 2;
            else if ((flags & 0x103) != 0 || numbers.GetValueOrDefault(42241UL) == 1) vipLevel = 1;
            else if (vipFlags is not null && numbers.ContainsKey(42241)) vipLevel = 0;
        }

        return new BotStranger(
            response.Body.Uin,
            Encoding.UTF8.GetString(nicknameBytes),
            response.Body.Uid,
            Encoding.UTF8.GetString(bytes.GetValueOrDefault(102UL) ?? []),
            Encoding.UTF8.GetString(bytes.GetValueOrDefault(103UL) ?? []),
            numbers.GetValueOrDefault(105UL),
            (BotGender)numbers.GetValueOrDefault(20009UL),
            checked((long)numbers.GetValueOrDefault(20026UL)),
            year is { } y && month is { } m && day is { } d ? new DateTimeOffset(y, m, d, 0, 0, 0, TimeSpan.Zero).ToUnixTimeSeconds() : null,
            numbers.GetValueOrDefault(20037UL),
            Encoding.UTF8.GetString(bytes.GetValueOrDefault(27394UL) ?? []),
            Encoding.UTF8.GetString(bytes.GetValueOrDefault(20003UL) ?? []),
            Encoding.UTF8.GetString(bytes.GetValueOrDefault(20004UL) ?? []),
            bytes.TryGetValue(20021, out byte[]? value) ? Encoding.UTF8.GetString(value) : null
        ) { BirthdayYear = year, BirthdayMonth = month, BirthdayDay = day,
            Vip = vipFlags is { } f ? (f & 1) != 0 : null, YearVip = vipFlags is { } yf ? (yf & 2) != 0 : null,
            SuperVip = vipFlags is { } sf ? (sf & 0x100) != 0 : null, VipLevel = vipLevel };
    }

    internal static (int? Year, int? Month, int? Day) ReadBirthday(byte[]? raw)
    {
        if (raw?.Length != 4) return (null, null, null);
        int year = BinaryPrimitives.ReadUInt16BigEndian(raw), month = raw[2], day = raw[3];
        if (year > 9999 || month is < 1 or > 12 || day < 1 || day > DateTime.DaysInMonth(year == 0 ? 2000 : year, month)) return (null, null, null);
        return (year == 0 ? null : year, month, day);
    }
}
