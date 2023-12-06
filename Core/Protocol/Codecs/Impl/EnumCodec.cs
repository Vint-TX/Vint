using Vint.Core.Protocol.Codecs.Buffer;

namespace Vint.Core.Protocol.Codecs.Impl;

public class EnumCodec(Type valueType) : Codec {
    public override void Encode(ProtocolBuffer buffer, object value) =>
        Protocol.GetCodec(new TypeCodecInfo(valueType)).Encode(buffer, value);

    public override object Decode(ProtocolBuffer buffer) =>
        Protocol.GetCodec(new TypeCodecInfo(valueType)).Decode(buffer);
}