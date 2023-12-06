using Vint.Core.Protocol.Codecs.Buffer;

namespace Vint.Core.Protocol.Codecs.Impl;

public class UShortCodec : Codec {
    public override void Encode(ProtocolBuffer buffer, object value) => buffer.Writer.Write((ushort)value);

    public override object Decode(ProtocolBuffer buffer) => buffer.Reader.ReadUInt16();
}