using FastEndpoints;
using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.Group;

public sealed class GetGroupTagsHandler(BotContext bot) : Endpoint<GetGroupTagsHandler.Request, MilkyApiResponse<GetGroupTagsHandler.Result>>
{
    public override void Configure() { AuthSchemes("Milky"); Post("/api/get_group_tags"); }
    public override async Task<MilkyApiResponse<Result>> ExecuteAsync(Request req, CancellationToken ct) => new(new Result { Tags = (await bot.GetGroupTags(req.GroupClass, ct)).Select(x => new Tag { Id = x.Id, Name = x.Name, Type = x.Type }).ToArray() });
    public sealed class Request { [JsonPropertyName("group_class")] public required uint GroupClass { get; init; } }
    public sealed class Result { [JsonPropertyName("tags")] public required Tag[] Tags { get; init; } }
    public sealed class Tag { [JsonPropertyName("id")] public uint Id { get; init; } [JsonPropertyName("name")] public required string Name { get; init; } [JsonPropertyName("type")] public uint Type { get; init; } }
}
public sealed class AddGroupTagHandler(BotContext bot) : Endpoint<AddGroupTagHandler.Request, MilkyApiResponse>
{
    public override void Configure() { AuthSchemes("Milky"); Post("/api/add_group_tag"); }
    public override async Task<MilkyApiResponse> ExecuteAsync(Request req, CancellationToken ct) { await bot.AddGroupTag(req.GroupClass, req.Name, ct); return new(); }
    public sealed class Request { [JsonPropertyName("group_class")] public required uint GroupClass { get; init; } [JsonPropertyName("name")] public required string Name { get; init; } }
}
public sealed class DeleteGroupTagHandler(BotContext bot) : Endpoint<DeleteGroupTagHandler.Request, MilkyApiResponse>
{
    public override void Configure() { AuthSchemes("Milky"); Post("/api/delete_group_tag"); }
    public override async Task<MilkyApiResponse> ExecuteAsync(Request req, CancellationToken ct) { await bot.DeleteGroupTag(req.GroupClass, req.Name, ct); return new(); }
    public sealed class Request { [JsonPropertyName("group_class")] public required uint GroupClass { get; init; } [JsonPropertyName("name")] public required string Name { get; init; } }
}
public sealed class GetGroupShareLinkHandler(BotContext bot) : Endpoint<GetGroupShareLinkHandler.Request, MilkyApiResponse<GetGroupShareLinkHandler.Result>>
{
    public override void Configure() { AuthSchemes("Milky"); Post("/api/get_group_share_link"); }
    public override async Task<MilkyApiResponse<Result>> ExecuteAsync(Request req, CancellationToken ct) => new(new Result { Url = await bot.GetGroupShareLink(req.GroupId, req.NeedShortUrl, req.AdditionalParam, ct) });
    public sealed class Request { [JsonPropertyName("group_id")] public required long GroupId { get; init; } [JsonPropertyName("need_short_url")] public bool NeedShortUrl { get; set; } = true; [JsonPropertyName("additional_param")] public string AdditionalParam { get; set; } = string.Empty; }
    public sealed class Result { [JsonPropertyName("url")] public required string Url { get; init; } }
}
public sealed class GetGroupAdminPermissionsHandler(BotContext bot) : Endpoint<GetGroupAdminPermissionsHandler.Request, MilkyApiResponse<GetGroupAdminPermissionsHandler.Result>>
{
    public override void Configure() { AuthSchemes("Milky"); Post("/api/get_group_admin_permissions"); }
    public override async Task<MilkyApiResponse<Result>> ExecuteAsync(Request req, CancellationToken ct)
    {
        var value = await bot.GetGroupAdminPermissions(req.GroupId, req.UserId, ct);
        return new(new Result { GroupId = value.GroupId, UserId = value.UserId, RecallMemberMessage = value.RecallMemberMessage, KickMember = value.KickMember, KickMemberPerDay = value.KickMemberPerDay, ReceiveVerify = value.ReceiveVerify });
    }
    public sealed class Request { [JsonPropertyName("group_id")] public required long GroupId { get; init; } [JsonPropertyName("user_id")] public required long UserId { get; init; } }
    public sealed class Result
    {
        [JsonPropertyName("group_id")] public long GroupId { get; init; }
        [JsonPropertyName("user_id")] public long UserId { get; init; }
        [JsonPropertyName("recall_member_message")] public bool? RecallMemberMessage { get; init; }
        [JsonPropertyName("kick_member")] public bool? KickMember { get; init; }
        [JsonPropertyName("kick_member_per_day")] public uint? KickMemberPerDay { get; init; }
        [JsonPropertyName("receive_verify")] public bool? ReceiveVerify { get; init; }
    }
}
public sealed class SetGroupAdminPermissionsHandler(BotContext bot) : Endpoint<SetGroupAdminPermissionsHandler.Request, MilkyApiResponse>
{
    public override void Configure() { AuthSchemes("Milky"); Post("/api/set_group_admin_permissions"); }
    public override async Task<MilkyApiResponse> ExecuteAsync(Request req, CancellationToken ct) { await bot.SetGroupAdminPermissions(req.GroupId, req.UserId, req.RecallMemberMessage, req.KickMember, req.KickMemberPerDay, ct); return new(); }
    public sealed class Request
    {
        [JsonPropertyName("group_id")] public required long GroupId { get; init; }
        [JsonPropertyName("user_id")] public required long UserId { get; init; }
        [JsonPropertyName("recall_member_message")] public bool? RecallMemberMessage { get; init; }
        [JsonPropertyName("kick_member")] public bool? KickMember { get; init; }
        [JsonPropertyName("kick_member_per_day")] public uint? KickMemberPerDay { get; init; }
    }
}
public sealed class SetGroupAdminReceiveVerifyHandler(BotContext bot) : Endpoint<SetGroupAdminReceiveVerifyHandler.Request, MilkyApiResponse>
{
    public override void Configure() { AuthSchemes("Milky"); Post("/api/set_group_admin_receive_verify"); }
    public override async Task<MilkyApiResponse> ExecuteAsync(Request req, CancellationToken ct) { await bot.SetGroupAdminReceiveVerify(req.GroupId, req.UserId, req.Enable, ct); return new(); }
    public sealed class Request { [JsonPropertyName("group_id")] public required long GroupId { get; init; } [JsonPropertyName("user_id")] public required long UserId { get; init; } [JsonPropertyName("enable")] public required bool Enable { get; init; } }
}
