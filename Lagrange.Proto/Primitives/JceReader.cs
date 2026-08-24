using System.Buffers.Binary;
using System.Text;

namespace Lagrange.Proto.Primitives;

public enum JceType : byte { Byte = 0, Short = 1, Int = 2, Long = 3, Float = 4, Double = 5, String1 = 6, String4 = 7, Map = 8, List = 9, StructBegin = 10, StructEnd = 11, Zero = 12, SimpleList = 13 }

public ref struct JceReader
{
    private readonly ReadOnlySpan<byte> _data;
    private int _offset;

    public JceReader(ReadOnlySpan<byte> data) { _data = data; _offset = 0; }
    public int Offset => _offset;
    public bool End => _offset >= _data.Length;

    public static T Parse<T>(ReadOnlySpan<byte> data) where T : IJcePackable<T> { var r = new JceReader(data); return T.Parse(ref r); }
    public JceHead PeekHead() { Ensure(1); var b = _data[_offset]; if ((b >> 4) == 15) { Ensure(2); return new((JceType)(b & 15), _data[_offset + 1], 2); } return new((JceType)(b & 15), b >> 4, 1); }
    public JceHead ReadHead() { var h = PeekHead(); _offset += h.Size; return h; }
    public bool TrySeek(int tag) { while (!End) { var h = PeekHead(); if (h.Tag >= tag || h.Type == JceType.StructEnd) return h.Tag == tag; SkipField(h.Type); } return false; }
    public void SkipField(JceType type)
    {
        ReadHeadValue(type);
    }
    private void ReadHeadValue(JceType type)
    {
        switch (type)
        {
            case JceType.Byte: Ensure(1); _offset++; break;
            case JceType.Short: Ensure(2); _offset += 2; break;
            case JceType.Int: case JceType.Float: Ensure(4); _offset += 4; break;
            case JceType.Long: case JceType.Double: Ensure(8); _offset += 8; break;
            case JceType.Zero: break;
            case JceType.String1: Skip(ReadByteRaw()); break;
            case JceType.String4: Skip(checked((int)ReadUInt32Raw())); break;
            case JceType.SimpleList: ReadHead(); Skip(ReadInt32(0)); break;
            case JceType.List: { var n = ReadInt32(0); for (var i = 0; i < n; i++) { var h = ReadHead(); ReadHeadValue(h.Type); } break; }
            case JceType.Map: { var n = ReadInt32(0); for (var i = 0; i < n * 2; i++) { var h = ReadHead(); ReadHeadValue(h.Type); } break; }
            case JceType.StructBegin: while (true) { var h = ReadHead(); if (h.Type == JceType.StructEnd) break; ReadHeadValue(h.Type); } break;
            case JceType.StructEnd: break;
            default: throw new InvalidDataException($"Unsupported JCE type {type}.");
        }
    }
    public byte ReadByte(int tag) { if (!TrySeek(tag)) return 0; var h = ReadHead(); return h.Type switch { JceType.Zero => (byte)0, JceType.Byte => ReadByteRaw(), JceType.Short => checked((byte)ReadInt16Raw()), _ => throw Bad(h, tag) }; }
    public short ReadInt16(int tag) { if (!TrySeek(tag)) return 0; var h = ReadHead(); return h.Type switch { JceType.Zero => 0, JceType.Byte => unchecked((sbyte)ReadByteRaw()), JceType.Short => ReadInt16Raw(), _ => throw Bad(h, tag) }; }
    public int ReadInt32(int tag) { if (!TrySeek(tag)) return 0; var h = ReadHead(); return h.Type switch { JceType.Zero => 0, JceType.Byte => unchecked((sbyte)ReadByteRaw()), JceType.Short => ReadInt16Raw(), JceType.Int => ReadInt32Raw(), _ => throw Bad(h, tag) }; }
    public long ReadInt64(int tag) { if (!TrySeek(tag)) return 0; var h = ReadHead(); return h.Type switch { JceType.Zero => 0, JceType.Byte => unchecked((sbyte)ReadByteRaw()), JceType.Short => ReadInt16Raw(), JceType.Int => ReadInt32Raw(), JceType.Long => ReadInt64Raw(), _ => throw Bad(h, tag) }; }
    public string ReadString(int tag) { if (!TrySeek(tag)) return string.Empty; var h = ReadHead(); var len = h.Type == JceType.String1 ? ReadByteRaw() : h.Type == JceType.String4 ? checked((int)ReadUInt32Raw()) : throw Bad(h, tag); Ensure(len); var s = Encoding.UTF8.GetString(_data.Slice(_offset, len)); _offset += len; return s; }
    public byte[] ReadBytes(int tag) { if (!TrySeek(tag)) return []; var h = ReadHead(); int len; if (h.Type == JceType.SimpleList) { var subtype = ReadHead(); if (subtype.Type != JceType.Byte) throw new InvalidDataException("JCE SIMPLE_LIST subtype must be BYTE."); len = ReadInt32(0); } else if (h.Type == JceType.List) len = ReadInt32(0); else throw Bad(h, tag); Ensure(len); var result = GC.AllocateUninitializedArray<byte>(len); if (h.Type == JceType.SimpleList) { _data.Slice(_offset, len).CopyTo(result); _offset += len; } else for (var i = 0; i < len; i++) result[i] = ReadByte(0); return result; }
    public T ReadStruct<T>(int tag) where T : IJcePackable<T>, new() { if (!TrySeek(tag)) return new T(); var h = ReadHead(); if (h.Type != JceType.StructBegin) throw Bad(h, tag); var value = T.Parse(ref this); var end = ReadHead(); if (end.Type != JceType.StructEnd) throw new InvalidDataException("JCE struct end expected."); return value; }
    public T[] ReadStructList<T>(int tag) where T : IJcePackable<T>, new() { if (!TrySeek(tag)) return []; var h = ReadHead(); if (h.Type != JceType.List) throw Bad(h, tag); var count = ReadInt32(0); var result = new T[count]; for (var i = 0; i < count; i++) result[i] = ReadStructValue<T>(); return result; }
    private T ReadStructValue<T>() where T : IJcePackable<T>, new() { var h = ReadHead(); if (h.Type != JceType.StructBegin) throw Bad(h, 0); var v = T.Parse(ref this); var end = ReadHead(); if (end.Type != JceType.StructEnd) throw new InvalidDataException("JCE struct end expected."); return v; }
    public Dictionary<string, Dictionary<string, byte[]>> ReadNestedByteMap(int tag) { if (!TrySeek(tag)) return []; var h = ReadHead(); if (h.Type != JceType.Map) throw Bad(h, tag); var n = ReadInt32(0); var result = new Dictionary<string, Dictionary<string, byte[]>>(n); for (var i = 0; i < n; i++) result[ReadString(0)] = ReadByteMap(1); return result; }
    private Dictionary<string, byte[]> ReadByteMap(int tag) { if (!TrySeek(tag)) return []; var h = ReadHead(); if (h.Type != JceType.Map) throw Bad(h, tag); var n = ReadInt32(0); var result = new Dictionary<string, byte[]>(n); for (var i = 0; i < n; i++) result[ReadString(0)] = ReadBytes(1); return result; }
    private byte ReadByteRaw() { Ensure(1); return _data[_offset++]; }
    private short ReadInt16Raw() { Ensure(2); var v = BinaryPrimitives.ReadInt16BigEndian(_data[_offset..]); _offset += 2; return v; }
    private int ReadInt32Raw() { Ensure(4); var v = BinaryPrimitives.ReadInt32BigEndian(_data[_offset..]); _offset += 4; return v; }
    private long ReadInt64Raw() { Ensure(8); var v = BinaryPrimitives.ReadInt64BigEndian(_data[_offset..]); _offset += 8; return v; }
    private uint ReadUInt32Raw() { Ensure(4); var v = BinaryPrimitives.ReadUInt32BigEndian(_data[_offset..]); _offset += 4; return v; }
    private void Skip(int count) { Ensure(count); _offset += count; }
    private void Ensure(int count) { if (count < 0 || count > _data.Length - _offset) throw new InvalidDataException("Truncated JCE payload."); }
    private static InvalidDataException Bad(JceHead h, int tag) => new($"Unexpected JCE type {h.Type} for tag {tag}.");
}

public readonly record struct JceHead(JceType Type, int Tag, int Size);
public interface IJcePackable<T> where T : IJcePackable<T> { static abstract T Parse(ref JceReader reader); }
