using System.Text;

namespace Vint.Core.Protocol.Codecs.Buffer;

public class BigEndianBinaryReader : BinaryReader {
    public BigEndianBinaryReader(Stream output) : base(output) { }

    public BigEndianBinaryReader(Stream output, Encoding encoding) : base(output, encoding) { }

    public BigEndianBinaryReader(Stream output, Encoding encoding, bool leaveOpen) : base(output, encoding, leaveOpen) { }

    public override short ReadInt16() {
        byte[] data = base.ReadBytes(2);
        Array.Reverse(data);
        return BitConverter.ToInt16(data);
    }

    public override int ReadInt32() {
        byte[] data = base.ReadBytes(4);
        Array.Reverse(data);
        return BitConverter.ToInt32(data);
    }

    public override long ReadInt64() {
        byte[] data = base.ReadBytes(8);
        Array.Reverse(data);
        return BitConverter.ToInt64(data);
    }

    public override ushort ReadUInt16() {
        byte[] data = base.ReadBytes(2);
        Array.Reverse(data);
        return BitConverter.ToUInt16(data);
    }

    public override uint ReadUInt32() {
        byte[] data = base.ReadBytes(4);
        Array.Reverse(data);
        return BitConverter.ToUInt32(data);
    }

    public override ulong ReadUInt64() {
        byte[] data = base.ReadBytes(8);
        Array.Reverse(data);
        return BitConverter.ToUInt64(data);
    }

    public override float ReadSingle() {
        byte[] data = base.ReadBytes(4);
        Array.Reverse(data);
        return BitConverter.ToSingle(data);
    }

    public override double ReadDouble() {
        byte[] data = base.ReadBytes(8);
        Array.Reverse(data);
        return BitConverter.ToSingle(data);
    }
}