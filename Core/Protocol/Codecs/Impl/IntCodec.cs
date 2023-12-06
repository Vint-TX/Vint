using Vint.Core.Protocol.Codecs.Buffer;

namespace Vint.Core.Protocol.Codecs.Impl;

public class IntCodec : Codec {
    public override void Encode(ProtocolBuffer buffer, object value) => buffer.Writer.Write((int)value);

    public override object Decode(ProtocolBuffer buffer) => buffer.Reader.ReadInt32();
}