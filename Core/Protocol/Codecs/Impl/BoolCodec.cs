using Vint.Core.Protocol.Codecs.Buffer;

namespace Vint.Core.Protocol.Codecs.Impl;

public class BoolCodec : Codec {
    public override void Encode(ProtocolBuffer buffer, object value) => buffer.Writer.Write((bool)value);

    public override object Decode(ProtocolBuffer buffer) => buffer.Reader.ReadBoolean();
}