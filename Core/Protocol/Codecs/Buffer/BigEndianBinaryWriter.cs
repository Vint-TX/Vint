using System.Text;

namespace Vint.Core.Protocol.Codecs.Buffer;

public class BigEndianBinaryWriter : BinaryWriter {
    public BigEndianBinaryWriter(Stream output) : base(output) { }

    public BigEndianBinaryWriter(Stream output, Encoding encoding) : base(output, encoding) { }

    public BigEndianBinaryWriter(Stream output, Encoding encoding, bool leaveOpen) : base(output, encoding, leaveOpen) { }

    public override void Write(short value) {
        byte[] data = BitConverter.GetBytes(value);
        Array.Reverse(data);
        base.Write(data);
    }

    public override void Write(int value) {
        byte[] data = BitConverter.GetBytes(value);
        Array.Reverse(data);
        base.Write(data);
    }

    public override void Write(long value) {
        byte[] data = BitConverter.GetBytes(value);
        Array.Reverse(data);
        base.Write(data);
    }

    public override void Write(ushort value) {
        byte[] data = BitConverter.GetBytes(value);
        Array.Reverse(data);
        base.Write(data);
    }

    public override void Write(uint value) {
        byte[] data = BitConverter.GetBytes(value);
        Array.Reverse(data);
        base.Write(data);
    }

    public override void Write(ulong value) {
        byte[] data = BitConverter.GetBytes(value);
        Array.Reverse(data);
        base.Write(data);
    }

    public override void Write(float value) {
        byte[] data = BitConverter.GetBytes(value);
        Array.Reverse(data);
        base.Write(data);
    }

    public override void Write(double value) {
        byte[] data = BitConverter.GetBytes(value);
        Array.Reverse(data);
        base.Write(data);
    }
}