using System.Text.Json;
using FastEndpoints;
using Lagrange.Milky.Api.Handlers.File;
using Lagrange.Milky.Api.Handlers.Group;
using Lagrange.Milky.Api.Handlers.Message;
using Lagrange.Milky.Api.Handlers.System;
using Lagrange_Milky;

namespace Lagrange.Milky.Test;

public sealed class MigrationRequestDefaultsTests
{
    private static T Read<T>(string json) where T : class
    {
        var options = new JsonSerializerOptions().AddSerializerContextsFromLagrange_Milky();
        return (T)JsonSerializer.Deserialize(json, options.GetTypeInfo(typeof(T)))!;
    }

    [Test]
    public void MissingNumericOptionalsRemainDistinctFromExplicitZero()
    {
        // STJ's init-only object factory used to assign zero over the property initializer.
        Assert.Multiple(() =>
        {
            Assert.That(Read<GetAiVoiceListHandler.Request>("{}").ChatType, Is.Null);
            Assert.That(Read<SendAiVoiceHandler.Request>("{}").ChatType, Is.Null);
            Assert.That(Read<SynthesizeAiVoiceHandler.Request>("{}").ChatType, Is.Null);
            Assert.That(Read<GetHistorySyncPageHandler.Request>("{}").Limit, Is.Null);
            Assert.That(Read<GetPrivateHistoryRoamPageHandler.Request>("{}").Limit, Is.Null);
            Assert.That(Read<GetFlashDownloadHandler.Request>("""{"fileset_uuid":"f"}""").FileIndex, Is.Null);
            Assert.That(Read<GetAiVoiceListHandler.Request>("""{"chat_type":0}""").ChatType, Is.Zero);
            Assert.That(Read<GetHistorySyncPageHandler.Request>("""{"limit":0}""").Limit, Is.Zero);
            Assert.That(Read<GetAiVoiceListHandler.Request>("""{"chat_type":2}""").ChatType, Is.EqualTo(2));
            Assert.That(Read<GetHistorySyncPageHandler.Request>("""{"limit":7}""").Limit, Is.EqualTo(7));
            Assert.That(Read<GetFlashDownloadHandler.Request>("""{"fileset_uuid":"f","file_index":2}""").FileIndex, Is.EqualTo(2));
        });
    }

    [TestCase("{}")]
    [TestCase("""{"group_ids":null,"private_targets":null}""")]
    public void EmptyHistoryProbeUsesEmptyCollections(string json)
    {
        var request = Read<ProbeHistorySyncStateHandler.Request>(json);
        Assert.That(request.GroupIds, Is.Empty);
        Assert.That(request.PrivateTargets, Is.Empty);
        Assert.That(Read<SetQzoneMessageVisibilityHandler.Request>("{}").Users, Is.Empty);
    }

    [Test]
    public void ReferenceDefaultsSurviveGeneratedJsonInitialization()
    {
        var audio = Read<ConvertRecordHandler.Request>("""{"file_uri":"base64://YQ=="}""");
        Assert.That(audio.Format, Is.EqualTo("wav"));
        Assert.That(Read<ConvertRecordHandler.Request>("""{"file_uri":"x","format":null}""").Format, Is.EqualTo("wav"));
        Assert.That(Read<ConvertRecordHandler.Request>("""{"file_uri":"x","format":"silk"}""").Format, Is.EqualTo("silk"));
        var video = Read<UploadGroupAlbumVideoHandler.Request>("""{"group_id":1,"album_id":"a","video_uri":"v","cover_uri":"c"}""");
        Assert.That(video.FileName, Is.EqualTo("video.mp4"));
        Assert.That(video.AlbumName, Is.Empty);
        Assert.That(video.DurationMilliseconds, Is.Null);
        video = Read<UploadGroupAlbumVideoHandler.Request>("""{"group_id":1,"album_id":"a","video_uri":"v","cover_uri":"c","file_name":"clip.mp4","duration_ms":500}""");
        Assert.That(video.FileName, Is.EqualTo("clip.mp4"));
        Assert.That(video.DurationMilliseconds, Is.EqualTo(500));
    }

    [Test]
    public void FastEndpointsGeneratedFactoriesHaveTheSameDefaultSemantics()
    {
        var cache = new ReflectionCache().AddFromLagrangeMilky();
        var history = (ProbeHistorySyncStateHandler.Request)cache[typeof(ProbeHistorySyncStateHandler.Request)].ObjectFactory!();
        var voice = (GetAiVoiceListHandler.Request)cache[typeof(GetAiVoiceListHandler.Request)].ObjectFactory!();
        var audio = (ConvertRecordHandler.Request)cache[typeof(ConvertRecordHandler.Request)].ObjectFactory!();
        var video = (UploadGroupAlbumVideoHandler.Request)cache[typeof(UploadGroupAlbumVideoHandler.Request)].ObjectFactory!();
        Assert.Multiple(() =>
        {
            Assert.That(history.GroupIds, Is.Empty);
            Assert.That(history.PrivateTargets, Is.Empty);
            Assert.That(voice.ChatType, Is.Null);
            Assert.That(audio.Format, Is.EqualTo("wav"));
            Assert.That(video.FileName, Is.EqualTo("video.mp4"));
            Assert.That(video.AlbumName, Is.Empty);
            Assert.That(video.DurationMilliseconds, Is.Null);
        });
    }
}
