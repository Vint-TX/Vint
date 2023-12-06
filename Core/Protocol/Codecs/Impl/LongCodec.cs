using Vint.Core.Protocol.Codecs.Buffer;

namespace Vint.Core.Protocol.Codecs.Impl;

public class LongCodec : Codec {
    public override void Encode(ProtocolBuffer buffer, object value) => buffer.Writer.Write((long)value);

    public override object Decode(ProtocolBuffer buffer) => buffer.Reader.ReadInt64();
}