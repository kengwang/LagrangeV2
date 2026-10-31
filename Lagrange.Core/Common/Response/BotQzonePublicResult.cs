using System.Text.Json;

namespace Lagrange.Core.Common.Response;

public sealed record BotQzonePublicFeedsPage(IReadOnlyList<JsonElement> Feeds, bool HasMore, string? NextCursor);
public sealed record BotQzoneShareArk(string Ark, bool RequiresConfirmation, string? Notice);
