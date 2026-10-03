namespace Lagrange.Core.Common.Response;

public sealed class BotGroupAnnouncementOptions
{
    public bool Pinned { get; init; }
    public bool SendToNewMembers { get; init; }
    public bool ShowEditCard { get; init; } = true;
    public bool ShowPopup { get; init; }
    public bool ConfirmRequired { get; init; } = true;
    public string? PictureId { get; init; }
    public int ImageWidth { get; init; } = 540;
    public int ImageHeight { get; init; } = 300;
}

public sealed class BotGroupAnnouncementImage
{
    public required string Id { get; init; }
    public required int Width { get; init; }
    public required int Height { get; init; }
}
