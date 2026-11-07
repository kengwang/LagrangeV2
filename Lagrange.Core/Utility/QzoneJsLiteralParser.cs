using System.Globalization;
using Acornima;
using Acornima.Ast;

namespace Lagrange.Core.Utility;

internal static class QzoneJsLiteralParser
{
    internal static System.Text.Json.Nodes.JsonObject ParseJsonResponse(string source) => ToJson(ParseResponse(source))?.AsObject() ?? throw new FormatException("Qzone response is not an object.");

    private static System.Text.Json.Nodes.JsonNode? ToJson(object? value) => value switch
    {
        null => null,
        Dictionary<string, object?> obj => new System.Text.Json.Nodes.JsonObject(obj.Select(x => new KeyValuePair<string, System.Text.Json.Nodes.JsonNode?>(x.Key, ToJson(x.Value)))),
        List<object?> array => new System.Text.Json.Nodes.JsonArray(array.Select(ToJson).ToArray()),
        string text => System.Text.Json.Nodes.JsonValue.Create(text),
        bool boolean => System.Text.Json.Nodes.JsonValue.Create(boolean),
        long integer when integer is >= int.MinValue and <= int.MaxValue => System.Text.Json.Nodes.JsonValue.Create((int)integer),
        long integer => System.Text.Json.Nodes.JsonValue.Create(integer),
        ulong integer => System.Text.Json.Nodes.JsonValue.Create(integer),
        double number => System.Text.Json.Nodes.JsonValue.Create(number),
        _ => throw new FormatException("Unsupported Qzone literal value.")
    };
    public static object? Parse(string source)
    {
        try { return Read(new Parser().ParseExpression(source), 0); }
        catch (ParseErrorException error) { throw new FormatException("Invalid Qzone literal.", error); }
    }

    internal static object? ParseResponse(string source)
    {
        if (source.TrimStart().StartsWith('{')) return Parse(source);
        try
        {
            var script = new Parser().ParseScript(source);
            if (script.Body.Count != 1 || script.Body[0] is not ExpressionStatement { Expression: CallExpression call } || call.Arguments.Count != 1)
                throw new FormatException("Qzone response must contain exactly one callback invocation.");
            return Read(call.Arguments[0], 0);
        }
        catch (ParseErrorException error) { throw new FormatException("Invalid Qzone response.", error); }
    }

    private static object? Read(Node node, int depth)
    {
        if (depth > 64) throw new FormatException("Qzone literal nesting exceeds the supported limit.");
        switch (node)
        {
            case ObjectExpression obj:
                var result = new Dictionary<string, object?>(StringComparer.Ordinal);
                foreach (var entry in obj.Properties)
                {
                    if (entry is not ObjectProperty { Computed: false, Method: false, Shorthand: false } property || property.Kind != PropertyKind.Init)
                        throw new FormatException("Only data properties are accepted in Qzone responses.");
                    var key = property.Key switch { Identifier identifier => identifier.Name, StringLiteral literal => literal.Value, NumericLiteral number => number.Raw, _ => throw new FormatException("Invalid Qzone property name.") };
                    var value = Read(property.Value, depth + 1);
                    if (key != "__proto__") result.Add(key, value);
                }
                return result;
            case ArrayExpression array:
                return array.Elements.Select(x => x is null ? null : Read(x, depth + 1)).ToList();
            case StringLiteral text: return text.Value;
            case BooleanLiteral boolean: return boolean.Value;
            case NullLiteral: return null;
            // Qzone JSONP uses undefined for empty fields and array placeholders.
            case Identifier { Name: "undefined" }: return null;
            case NumericLiteral number:
                if (long.TryParse(number.Raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var integer)) return integer;
                if (ulong.TryParse(number.Raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var unsigned)) return unsigned;
                if (!double.IsFinite(number.Value)) throw new FormatException("Non-finite Qzone number.");
                if (Math.Abs(number.Value) > 9007199254740991 && Math.Truncate(number.Value) == number.Value) throw new FormatException("Qzone integer cannot be represented exactly.");
                return number.Value;
            case UnaryExpression unary when unary.Operator == Operator.UnaryNegation && unary.Argument is NumericLiteral number:
                if (long.TryParse("-" + number.Raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var negative)) return negative;
                return -number.Value;
            default: throw new FormatException("Executable expressions are not accepted in Qzone responses.");
        }
    }
}
