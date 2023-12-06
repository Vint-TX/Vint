using Vint.Core.Protocol.Codecs.Buffer;

namespace Vint.Core.Protocol.Codecs.Impl;

public class FloatCodec : Codec {
    public override void Encode(ProtocolBuffer buffer, object value) => buffer.Writer.Write((float)value);

    public override object Decode(ProtocolBuffer buffer) => buffer.Reader.ReadSingle();
}