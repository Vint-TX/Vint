using Vint.Core.Protocol.Codecs.Buffer;

namespace Vint.Core.Protocol.Codecs.Impl;

public class UIntCodec : Codec {
    public override void Encode(ProtocolBuffer buffer, object value) => buffer.Writer.Write((uint)value);

    public override object Decode(ProtocolBuffer buffer) => buffer.Reader.ReadUInt32();
}