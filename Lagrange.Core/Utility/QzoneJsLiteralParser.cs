namespace Lagrange.Core.Utility;

internal static class QzoneJsLiteralParser
{
    public static object? Parse(string source)
    {
        var parser = new Parser(source); return parser.Value();
    }
    private sealed class Parser(string source)
    {
        private int _index;
        private void Skip() { while (_index < source.Length && char.IsWhiteSpace(source[_index])) _index++; }
        internal object? Value()
        {
            Skip(); if (_index >= source.Length) throw new FormatException("Unexpected end of QZone payload.");
            return source[_index] switch { '{' => Object(), '[' => Array(), '\'' or '"' => String(), _ => Token() };
        }
        private Dictionary<string, object?> Object()
        {
            _index++; var result = new Dictionary<string, object?>(StringComparer.Ordinal); Skip();
            if (_index < source.Length && source[_index] == '}') { _index++; return result; }
            while (true)
            {
                Skip(); var key = source[_index] is '\'' or '"' ? String() as string ?? string.Empty : Identifier(); Skip();
                if (_index >= source.Length || source[_index++] != ':') throw new FormatException("Invalid QZone object.");
                var value = Value(); if (key != "__proto__") result[key] = value; Skip();
                if (_index >= source.Length) throw new FormatException("Invalid QZone object.");
                if (source[_index] == '}') { _index++; return result; }
                if (source[_index++] != ',') throw new FormatException("Invalid QZone object separator.");
                Skip(); if (_index < source.Length && source[_index] == '}') { _index++; return result; }
            }
        }
        private List<object?> Array()
        {
            _index++; var result = new List<object?>(); Skip();
            if (_index < source.Length && source[_index] == ']') { _index++; return result; }
            while (true)
            {
                result.Add(Value()); Skip();
                if (_index >= source.Length) throw new FormatException("Invalid QZone array.");
                if (source[_index] == ']') { _index++; return result; }
                if (source[_index++] != ',') throw new FormatException("Invalid QZone array separator.");
                Skip(); if (_index < source.Length && source[_index] == ']') { _index++; return result; }
            }
        }
        private string Identifier() { var start = _index; while (_index < source.Length && (char.IsLetterOrDigit(source[_index]) || source[_index] is '_' or '$')) _index++; if (start == _index) throw new FormatException("Invalid QZone key."); return source[start.._index]; }
        private string String()
        {
            var quote = source[_index++]; var result = new System.Text.StringBuilder();
            while (_index < source.Length)
            {
                var c = source[_index++]; if (c == quote) return result.ToString();
                if (c != '\\') { result.Append(c); continue; }
                if (_index >= source.Length) break; var e = source[_index++];
                if (e == 'x' && _index + 2 <= source.Length && int.TryParse(source[_index..(_index + 2)], System.Globalization.NumberStyles.HexNumber, null, out var x)) { result.Append((char)x); _index += 2; }
                else if (e == 'u' && _index + 4 <= source.Length && int.TryParse(source[_index..(_index + 4)], System.Globalization.NumberStyles.HexNumber, null, out var u)) { result.Append((char)u); _index += 4; }
                else result.Append(e switch { 'n' => '\n', 'r' => '\r', 't' => '\t', 'b' => '\b', 'f' => '\f', 'v' => '\v', _ => e });
            }
            throw new FormatException("Unterminated QZone string.");
        }
        private object? Token()
        {
            var start = _index; while (_index < source.Length && !char.IsWhiteSpace(source[_index]) && ",}]".IndexOf(source[_index]) < 0) _index++;
            var token = source[start.._index]; if (token is "undefined" or "null") return null; if (token is "true") return true; if (token is "false") return false;
            if (double.TryParse(token, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var number)) return number;
            return token;
        }
    }
}
