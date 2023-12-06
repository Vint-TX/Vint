using Vint.Core.Protocol.Codecs.Buffer;

namespace Vint.Core.Protocol.Codecs.Impl;

public class SByteCodec : Codec {
    public override void Encode(ProtocolBuffer buffer, object value) => buffer.Writer.Write((sbyte)value);

    public override object Decode(ProtocolBuffer buffer) => buffer.Reader.ReadSByte();
}