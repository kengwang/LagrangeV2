using Lagrange.Core.Common;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Services;
namespace Lagrange.Core.Internal.Services.System;
[EventSubscribe<FlashTransferPrepareReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x12a9_100")]
internal sealed class FlashTransferPrepareService : OidbService<FlashTransferPrepareReq, FlashTransferPrepareResp, MigrationFlashPrepareUploadReq, MigrationFlashPrepareUploadResp>
{
    protected override uint Command => 0x12a9;
    protected override uint Service => 100;
    protected override uint Reserved => 1;
    protected override Task<MigrationFlashPrepareUploadReq> ProcessRequest(FlashTransferPrepareReq request, BotContext context) => Task.FromResult(request.Body);
    protected override Task<FlashTransferPrepareResp> ProcessResponse(MigrationFlashPrepareUploadResp response, BotContext context) => Task.FromResult(new FlashTransferPrepareResp(response));
}
[EventSubscribe<FlashTransferApplyReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x12a9_103")]
internal sealed class FlashTransferApplyService : OidbService<FlashTransferApplyReq, FlashTransferApplyResp, MigrationFlashApplyUploadReq, DEmptyResp>
{
    protected override uint Command => 0x12a9;
    protected override uint Service => 103;
    protected override uint Reserved => 1;
    protected override Task<MigrationFlashApplyUploadReq> ProcessRequest(FlashTransferApplyReq request, BotContext context) => Task.FromResult(request.Body);
    protected override Task<FlashTransferApplyResp> ProcessResponse(DEmptyResp response, BotContext context) => Task.FromResult(new FlashTransferApplyResp(response));
}
[EventSubscribe<FlashTransferDownloadMetadataReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x93d4_1")]
internal sealed class FlashTransferDownloadMetadataService : OidbService<FlashTransferDownloadMetadataReq, FlashTransferDownloadMetadataResp, MigrationFlashGetDownloadUrlReq, MigrationFlashGetDownloadUrlResp>
{
    protected override uint Command => 0x93d4;
    protected override uint Service => 1;
    protected override uint Reserved => 0;
    protected override Task<MigrationFlashGetDownloadUrlReq> ProcessRequest(FlashTransferDownloadMetadataReq request, BotContext context) => Task.FromResult(request.Body);
    protected override Task<FlashTransferDownloadMetadataResp> ProcessResponse(MigrationFlashGetDownloadUrlResp response, BotContext context) => Task.FromResult(new FlashTransferDownloadMetadataResp(response));
}
[EventSubscribe<FlashTransferDownloadReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x12a9_200")]
internal sealed class FlashTransferDownloadService : OidbService<FlashTransferDownloadReq, FlashTransferDownloadResp, MigrationFlashGetDownloadReq, MigrationFlashGetDownloadResp>
{
    protected override uint Command => 0x12a9;
    protected override uint Service => 200;
    protected override uint Reserved => 1;
    protected override Task<MigrationFlashGetDownloadReq> ProcessRequest(FlashTransferDownloadReq request, BotContext context) => Task.FromResult(request.Body);
    protected override Task<FlashTransferDownloadResp> ProcessResponse(MigrationFlashGetDownloadResp response, BotContext context) => Task.FromResult(new FlashTransferDownloadResp(response));
}
