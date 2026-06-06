using System.Buffers.Binary;
using Lagrange.Core.Common;
using Lagrange.Core.Internal.Events;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Service;

namespace Lagrange.Core.Internal.Services.System;

[EventSubscribe<FetchPinsEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x12b3_0")]
internal class FetchPinsService : OidbService<FetchPinsEventReq, FetchPinsEventResp, FetchPinsRequest, FetchPinsResponse>
{
    private protected override uint Command => 0x12b3;

    private protected override uint Service => 0;

    private protected override Task<FetchPinsRequest> ProcessRequest(FetchPinsEventReq request, BotContext context)
    {
        return Task.FromResult(new FetchPinsRequest());
    }

    private protected override Task<FetchPinsEventResp> ProcessResponse(FetchPinsResponse response, BotContext context)
    {
        return Task.FromResult(new FetchPinsEventResp(
            response.Friends?.Select(friend => friend.Uid).ToList() ?? [],
            response.Groups?.Select(group => group.Uin).ToList() ?? []
        ));
    }
}

[EventSubscribe<SetPinFriendEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x5d6_18")]
internal class SetPinFriendService : OidbService<SetPinFriendEventReq, SetPinFriendEventResp, SetPinFriendRequest, SetPinResponse>
{
    private protected override uint Command => 0x5d6;

    private protected override uint Service => 18;

    private protected override Task<SetPinFriendRequest> ProcessRequest(SetPinFriendEventReq request, BotContext context)
    {
        return Task.FromResult(new SetPinFriendRequest
        {
            Field1 = 0,
            Info = new SetPinFriendRequestInfo
            {
                FriendUid = request.Uid,
                Field400 = new SetPinField400
                {
                    Field1 = 13578,
                    Timestamp = request.IsPin ? GetTimestamp() : []
                }
            },
            Field3 = 1
        });
    }

    private protected override Task<SetPinFriendEventResp> ProcessResponse(SetPinResponse response, BotContext context)
    {
        return Task.FromResult(SetPinFriendEventResp.Default);
    }

    private static byte[] GetTimestamp()
    {
        byte[] timestamp = new byte[4];
        BinaryPrimitives.WriteInt32BigEndian(timestamp, (int)DateTimeOffset.Now.ToUnixTimeSeconds());
        return timestamp;
    }
}

[EventSubscribe<SetPinGroupEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x5d6_1")]
internal class SetPinGroupService : OidbService<SetPinGroupEventReq, SetPinGroupEventResp, SetPinGroupRequest, SetPinResponse>
{
    private protected override uint Command => 0x5d6;

    private protected override uint Service => 1;

    private protected override Task<SetPinGroupRequest> ProcessRequest(SetPinGroupEventReq request, BotContext context)
    {
        return Task.FromResult(new SetPinGroupRequest
        {
            Field1 = 0,
            Info = new SetPinGroupRequestInfo
            {
                GroupUin = request.GroupUin,
                Field400 = new SetPinField400
                {
                    Field1 = 13569,
                    Timestamp = request.IsPin ? GetTimestamp() : []
                }
            },
            Field3 = 11
        });
    }

    private protected override Task<SetPinGroupEventResp> ProcessResponse(SetPinResponse response, BotContext context)
    {
        return Task.FromResult(SetPinGroupEventResp.Default);
    }

    private static byte[] GetTimestamp()
    {
        byte[] timestamp = new byte[4];
        BinaryPrimitives.WriteInt32BigEndian(timestamp, (int)DateTimeOffset.Now.ToUnixTimeSeconds());
        return timestamp;
    }
}
