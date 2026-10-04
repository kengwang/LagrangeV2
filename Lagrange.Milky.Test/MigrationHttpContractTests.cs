using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FastEndpoints;
using Lagrange.Core.Common;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api.Handlers.System;
using Lagrange.Milky.Caching;
using Lagrange.Milky.Configurations;
using Lagrange.Milky.Converters;
using Lagrange.Milky.Security;
using Lagrange.Milky.Extensions;
using Lagrange_Milky;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
namespace Lagrange.Milky.Test;

public sealed class MigrationHttpContractTests
{
    [Test]
    public async Task ExtensionRoutesRequireMilkyAuthenticationAndReturnGeneratedDeviceDto()
    {
        string[] handlers = ["GetOnlineDevicesHandler", "RequestDatabaseKeyHandler", "GetUserStatusHandler", "GetMiniAppArkHandler",
            "SetQzoneMessageVisibilityHandler", "ClickInlineKeyboardButtonHandler", "GetGroupTodoListHandler", "GetDownloadRKeysHandler",
            "SendTempMessageHandler", "GetAiVoiceListHandler", "SynthesizeAiVoiceHandler", "SendAiVoiceHandler", "TranscribeVoiceHandler",
            "UploadFlashFilesHandler", "GetFlashDownloadHandler", "GetFlashShareLinkHandler", "ResolveFlashShareCodeHandler", "ConvertRecordHandler",
            "UploadGroupAlbumVideoHandler", "ProbeHistorySyncStateHandler", "GetHistorySyncPageHandler", "GetPrivateHistoryRoamPageHandler"];
        string[] routes = ["get_online_devices", "request_database_key", "get_user_status", "get_mini_app_ark",
            "set_qzone_message_visibility", "click_inline_keyboard_button", "get_group_todo_list", "get_download_rkeys",
            "send_temp_message", "get_ai_voice_list", "synthesize_ai_voice", "send_ai_voice", "transcribe_voice",
            "upload_flash_files", "get_flash_download", "get_flash_share_link", "resolve_flash_share_code", "convert_record",
            "upload_group_album_video", "probe_history_sync_state", "get_history_sync_page", "get_private_history_roam_page"];
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        using var bot = BotFactory.Create(new BotConfig(), new BotKeystore(), new BotAppInfo());
        builder.Services.AddSingleton(bot);
        builder.Services.AddSingleton(new MilkyConfiguration(new("127.0.0.1", 0), "test-token"));
        builder.Services.AddSingleton<MessageCache>();
        builder.Services.AddSingleton<MilkyConverter>();
        builder.Services.AddSingleton<ResourceConverter>();
        builder.Services.AddAuthentication("Milky").AddScheme<AuthenticationSchemeOptions, MilkyAuthenticationHandler>("Milky", _ => { });
        builder.Services.AddAuthorization();
        builder.Services.AddFastEndpoints(options =>
        {
            options.DisableAutoDiscovery = true;
            options.Assemblies = [typeof(GetOnlineDevicesHandler).Assembly];
            options.Filter = type => handlers.Contains(type.Name);
        });
        await using var app = builder.Build();
        app.UseAuthentication(); app.UseAuthorization();
        app.UseFastEndpoints(options =>
        {
            options.ConfigureMilkyBinding();
        });
        await app.StartAsync();
        try
        {
            using var client = new HttpClient { BaseAddress = new Uri(app.Urls.Single()) };
            foreach (string route in routes)
            {
                using var response = await client.PostAsync("/api/" + route, new StringContent("{}", Encoding.UTF8, "application/json"));
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized), route);
            }
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test-token");
            using var accepted = await client.PostAsync("/api/get_online_devices", new StringContent("{}", Encoding.UTF8, "application/json"));
            Assert.That(accepted.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            using var data = JsonDocument.Parse(await accepted.Content.ReadAsStringAsync());
            Assert.That(data.RootElement.GetProperty("retcode").GetInt64(), Is.Zero);
            Assert.That(data.RootElement.GetProperty("data").GetProperty("observed").GetBoolean(), Is.False);
            Assert.That(data.RootElement.GetProperty("data").GetProperty("devices").ValueKind, Is.EqualTo(JsonValueKind.Null));
        }
        finally { await app.StopAsync(); }
    }
}
