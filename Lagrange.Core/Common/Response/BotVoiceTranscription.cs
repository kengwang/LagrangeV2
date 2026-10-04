namespace Lagrange.Core.Common.Response;
/// <summary>Identifies a received voice message for QQ speech recognition.</summary>
/// <param name="IsGroup">Whether the recording belongs to a group conversation.</param>
/// <param name="MessageId">A nonzero request correlation key echoed by the transcription push; not the NT MsgUid.</param>
/// <param name="PeerUin">The group identifier, or the original private message receiver.</param>
/// <param name="SenderUin">The original recording sender.</param>
/// <param name="FileUuid">The recording's server media UUID.</param>
/// <param name="Md5">The recording's 32-character hexadecimal MD5.</param>
/// <param name="Duration">The recording duration in seconds.</param>
/// <param name="Size">The recording size in bytes.</param>
/// <param name="Format">The server media voice format.</param>
/// <param name="FileId">The optional legacy group recording file identifier.</param>
public sealed record BotVoiceTranscription(bool IsGroup, ulong MessageId, long PeerUin, long SenderUin,
    string FileUuid, string Md5, uint Duration, uint Size, uint Format, uint FileId = 0);
/// <summary>Server-generated AI voice media metadata.</summary>
public sealed record BotAiVoiceMedia(string FileUuid, string FileName, uint Size, uint Duration, uint Format, string Url);
