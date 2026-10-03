using Lagrange.Core.Utility;

namespace Lagrange.Core.Test.Protocol;

public class QzoneParserTest
{
    [Test]
    public void ParsesObjectsArraysAndEscapes()
    {
        var value = (Dictionary<string, object?>)QzoneJsLiteralParser.Parse("{foo: 'bar\\n', list: [1, true, null]}")!;
        Assert.That(value["foo"], Is.EqualTo("bar\n"));
        Assert.That((List<object?>)value["list"]!, Has.Count.EqualTo(3));
    }

    [Test]
    public void IgnoresPrototypeProperty()
    {
        var value = (Dictionary<string, object?>)QzoneJsLiteralParser.Parse("{__proto__: {polluted: true}, ok: 1}")!;
        Assert.That(value.ContainsKey("__proto__"), Is.False);
        Assert.That(value["ok"], Is.EqualTo(1d));
    }

    [Test]
    public void ParsesHexAndUnicodeStrings()
    {
        var value = (Dictionary<string, object?>)QzoneJsLiteralParser.Parse("{x: '\\x41', y: '\\u4E2D'}")!;
        Assert.That(value["x"], Is.EqualTo("A"));
        Assert.That(value["y"], Is.EqualTo("中"));
    }

    [Test]
    public void RejectsMalformedPayload()
    {
        Assert.Throws<FormatException>(() => QzoneJsLiteralParser.Parse("{broken"));
    }
}
