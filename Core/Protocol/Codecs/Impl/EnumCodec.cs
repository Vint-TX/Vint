using Vint.Core.Protocol.Codecs.Buffer;
using Vint.Core.Protocol.Codecs.Info;

namespace Vint.Core.Protocol.Codecs.Impl;

public class EnumCodec(
    Type enumType
) : Codec {
    public override void Encode(ProtocolBuffer buffer, object value) {
        TypeCode? typeCode = (value as Enum)?.GetTypeCode();

        if (typeCode != TypeCode.Byte)
            throw new ArgumentException($"Enum TypeCode must be Byte. Current: {typeCode} ({value.GetType().Name})");

        Protocol.GetCodec(typeof(byte)).Encode(buffer, value);
    }

    public override object Decode(ProtocolBuffer buffer) =>
        Enum.ToObject(enumType, Protocol.GetCodec(typeof(byte)).Decode(buffer));
}
