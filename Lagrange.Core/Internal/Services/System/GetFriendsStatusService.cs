using System.Globalization;
using System.Text;
using Lagrange.Core.Common;
using Lagrange.Core.Common.Response;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Services;

namespace Lagrange.Core.Internal.Services.System;

[EventSubscribe<GetFriendsStatusEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x116d_1")]
internal sealed class GetFriendsStatusService
    : OidbService<GetFriendsStatusEventReq, GetFriendsStatusEventResp, FriendsStatusRequest, FriendsStatusResponse>
{
    private static readonly UTF8Encoding Utf8 = new(false, true);

    protected override uint Command => 0x116d;
    protected override uint Service => 1;

    protected override Task<FriendsStatusRequest> ProcessRequest(GetFriendsStatusEventReq request, BotContext context)
    {
        string uid = context.Keystore.Uid;
        if (string.IsNullOrWhiteSpace(uid))
            throw new InvalidOperationException("The current session has no UID for the friends-status request.");
        return Task.FromResult(new FriendsStatusRequest { SelfUid = uid });
    }

    protected override Task<GetFriendsStatusEventResp> ProcessResponse(FriendsStatusResponse response, BotContext context)
    {
        var friends = response.Friends.Select(entry => ParseEntry(entry.Span)).ToArray();
        return Task.FromResult(new GetFriendsStatusEventResp(new BotFriendsStatusResult(response.SelfUid, friends)));
    }

    private static BotFriendStatus ParseEntry(ReadOnlySpan<byte> entry)
    {
        var reader = new EntryReader(entry);
        string uid = string.Empty;
        uint? status = null;
        string statusText = string.Empty;
        var raw = new List<BotFriendStatusRawField>();
        while (!reader.IsCompleted)
        {
            ulong tag = reader.ReadVarInt();
            if (tag > uint.MaxValue || tag >> 3 == 0)
                throw new InvalidDataException("Invalid friends-status field number.");
            uint field = (uint)(tag >> 3);
            byte wireType = (byte)(tag & 7);
            ulong number = 0;
            ReadOnlySpan<byte> bytes = default;
            switch (wireType)
            {
                case 0: number = reader.ReadVarInt(); break;
                case 1: bytes = reader.ReadBytes(8); break;
                case 2: bytes = reader.ReadLengthDelimited(); break;
                case 5: bytes = reader.ReadBytes(4); break;
                default: throw new InvalidDataException("Unsupported friends-status wire type.");
            }

            switch (field)
            {
                case 1 when wireType == 2: uid = Utf8.GetString(bytes); break;
                case 2 when wireType == 0 && number <= uint.MaxValue: status = (uint)number; break;
                case 15 when wireType == 2: statusText = Utf8.GetString(bytes); break;
                case 1 or 2 or 15: throw new InvalidDataException("Invalid friends-status known field encoding.");
                default:
                    raw.Add(new BotFriendStatusRawField(field, wireType,
                        wireType == 0 ? number.ToString(CultureInfo.InvariantCulture) : Convert.ToHexString(bytes)));
                    break;
            }
        }
        if (string.IsNullOrWhiteSpace(uid)) throw new InvalidDataException("Friends-status entry has no UID.");
        return new BotFriendStatus(uid, status, statusText, raw);
    }

    // Validate boundaries locally: entries may contain arbitrary unknown fields.
    private ref struct EntryReader(ReadOnlySpan<byte> data)
    {
        private readonly ReadOnlySpan<byte> _data = data;
        private int _offset;

        public readonly bool IsCompleted => _offset == _data.Length;

        public ulong ReadVarInt()
        {
            ulong value = 0;
            for (int index = 0; index < 10; index++)
            {
                if (_offset == _data.Length) throw new InvalidDataException("Truncated friends-status varint.");
                byte part = _data[_offset++];
                if (index == 9 && part > 1) throw new InvalidDataException("Overflowing friends-status varint.");
                value |= (ulong)(part & 0x7f) << (index * 7);
                if ((part & 0x80) == 0) return value;
            }
            throw new InvalidDataException("Invalid friends-status varint.");
        }

        public ReadOnlySpan<byte> ReadLengthDelimited()
        {
            ulong length = ReadVarInt();
            if (length > int.MaxValue) throw new InvalidDataException("Friends-status field length is too large.");
            return ReadBytes((int)length);
        }

        public ReadOnlySpan<byte> ReadBytes(int length)
        {
            if (length > _data.Length - _offset) throw new InvalidDataException("Truncated friends-status field.");
            var result = _data.Slice(_offset, length);
            _offset += length;
            return result;
        }
    }
}
