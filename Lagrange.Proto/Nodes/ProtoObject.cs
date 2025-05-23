using System.Buffers;
using Lagrange.Proto.Primitives;
using Lagrange.Proto.Serialization;
using Lagrange.Proto.Serialization.Converter;
using Lagrange.Proto.Serialization.Metadata;
using Lagrange.Proto.Utility;

namespace Lagrange.Proto.Nodes;

public partial class ProtoObject() : ProtoNode(WireType.LengthDelimited)
{
    public override void WriteTo(int field, ProtoWriter writer)
    {
        writer.EncodeVarInt(Measure(field));
        
        foreach (var (f, node) in _fields)
        {
            writer.EncodeVarInt(f << 3 | (int)node.WireType);
            node.WriteTo(f, writer);
        }
    }

    public override int Measure(int field)
    {
        int size = 0;
        foreach (var (f, node) in _fields)
        {
            size += ProtoHelper.GetVarIntLength(f << 3 | (int)node.WireType);
            size += node.Measure(f);
        }
        return size;
    }

    private protected override ProtoNode GetItem(int field)
    {
        if (_fields.TryGetValue(field, out var node)) return node;

        throw new KeyNotFoundException($"Field {field} not found in ProtoObject.");
    }

    private protected override void SetItem(int field, ProtoNode node)
    {
        if (_fields.TryGetValue(field, out var removed))
        {
            if (removed is not ProtoArray array)
            {
                DetachParent(removed);
                array = new ProtoArray(removed.WireType, removed, node);
                node = array;
            }
            else
            {
                array.Add(node);
                node.AssignParent(array);
                return;
            }
        }
        
        _fields[field] = node;
        node.AssignParent(this);
    }

    public static ProtoObject Parse(ReadOnlySpan<byte> bytes)
    {
        var reader = new ProtoReader(bytes);
        var obj = new ProtoObject();
        var converter = ProtoTypeResolver.GetConverter<ProtoRawValue>();

        while (!reader.IsCompleted)
        {
            int tag = reader.DecodeVarInt<int>();
            int field = tag >> 3;
            var wireType = (WireType)(tag & 0x7);

            var rawValue = converter.Read(field, wireType, ref reader);
            obj[field] = new ProtoValue<ProtoRawValue>(rawValue, wireType);
        }
        
        return obj;
    }
    
    public byte[] Serialize()
    {
        var writer = ProtoWriterCache.RentWriterAndBuffer(512, out var buffer);
        try
        {
            foreach (var (f, node) in _fields)
            {
                writer.EncodeVarInt(f << 3 | (int)node.WireType);
                node.WriteTo(f, writer);
            }
            writer.Flush();

            return buffer.ToArray();
        }
        finally
        {
            ProtoWriterCache.ReturnWriterAndBuffer(writer, buffer);
        }
    }

    public void Serialize(IBufferWriter<byte> buffer)
    {
        var writer = ProtoWriterCache.RentWriter(buffer);
        try
        {
            foreach (var (f, node) in _fields)
            {
                writer.EncodeVarInt(f << 3 | (int)node.WireType);
                node.WriteTo(f, writer);
            } 
            writer.Flush();
        }
        finally
        {
            ProtoWriterCache.ReturnWriter(writer);
        }
    }
}