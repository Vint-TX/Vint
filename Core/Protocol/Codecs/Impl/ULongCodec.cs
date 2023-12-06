using Vint.Core.Protocol.Codecs.Buffer;

namespace Vint.Core.Protocol.Codecs.Impl;

public class ULongCodec : Codec {
    public override void Encode(ProtocolBuffer buffer, object value) => buffer.Writer.Write((ulong)value);

    public override object Decode(ProtocolBuffer buffer) => buffer.Reader.ReadUInt64();
}