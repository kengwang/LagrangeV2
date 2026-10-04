using System.Text.Json;
using Lagrange.Core.Internal.Services.Http;
namespace Lagrange.Core.Test.Protocol;
public sealed class QzoneVisibilityTest
{
    [Test]
    public void PreservesPostContentAndPictureIdentity()
    {
        using var detail = JsonDocument.Parse("""{"tid":"post","uin":123,"content":"fallback","conlist":[{"con":" hello "},{"con":"world "}],"pic":[{"pic_id":"0,a,b","height":10,"width":20,"url1":"https://example.test/p?bo=keep"}]}""");
        var fields = QzoneVisibilityHttpService.BuildUpdateFields(detail.RootElement, 123, "post");
        Assert.That(fields["con"], Is.EqualTo(" hello world "));
        Assert.That(fields["richval"], Is.EqualTo(",a,b,b,22,10,20,,0,0"));
        Assert.That(fields["pic_bo"], Is.EqualTo("keep\tkeep"));
    }
    [TestCase("{\"tid\":\"other\",\"uin\":123}")]
    [TestCase("{\"tid\":\"post\",\"uin\":456}")]
    [TestCase("{\"tid\":\"post\"}")]
    [TestCase("{\"tid\":\"post\",\"uin\":123,\"pic\":[{\"pic_id\":\"incomplete\"}]}")]
    public void RejectsUnverifiablePostOrIncompleteMedia(string json)
    {
        using var detail = JsonDocument.Parse(json);
        Assert.Throws<InvalidOperationException>(() => QzoneVisibilityHttpService.BuildUpdateFields(detail.RootElement, 123, "post"));
    }
}
