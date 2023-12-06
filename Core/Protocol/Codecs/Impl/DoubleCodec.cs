using Vint.Core.Protocol.Codecs.Buffer;

namespace Vint.Core.Protocol.Codecs.Impl;

public class DoubleCodec : Codec {
    public override void Encode(ProtocolBuffer buffer, object value) => buffer.Writer.Write((double)value);

    public override object Decode(ProtocolBuffer buffer) => buffer.Reader.ReadDouble();
}