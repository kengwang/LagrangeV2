using Lagrange.Core.Events;
using Lagrange.Core.Internal.Packets.Service;
namespace Lagrange.Core.Internal.Events.System;
internal sealed class FlashTransferPrepareReq(MigrationFlashPrepareUploadReq body) : ProtocolEvent { public MigrationFlashPrepareUploadReq Body { get; } = body; }
internal sealed class FlashTransferPrepareResp(MigrationFlashPrepareUploadResp body) : ProtocolEvent { public MigrationFlashPrepareUploadResp Body { get; } = body; }
internal sealed class FlashTransferApplyReq(MigrationFlashApplyUploadReq body) : ProtocolEvent { public MigrationFlashApplyUploadReq Body { get; } = body; }
internal sealed class FlashTransferApplyResp(DEmptyResp body) : ProtocolEvent { public DEmptyResp Body { get; } = body; }
internal sealed class FlashTransferDownloadMetadataReq(MigrationFlashGetDownloadUrlReq body) : ProtocolEvent { public MigrationFlashGetDownloadUrlReq Body { get; } = body; }
internal sealed class FlashTransferDownloadMetadataResp(MigrationFlashGetDownloadUrlResp body) : ProtocolEvent { public MigrationFlashGetDownloadUrlResp Body { get; } = body; }
internal sealed class FlashTransferDownloadReq(MigrationFlashGetDownloadReq body) : ProtocolEvent { public MigrationFlashGetDownloadReq Body { get; } = body; }
internal sealed class FlashTransferDownloadResp(MigrationFlashGetDownloadResp body) : ProtocolEvent { public MigrationFlashGetDownloadResp Body { get; } = body; }
