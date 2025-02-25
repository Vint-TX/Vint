﻿using Vint.Core.Server.Game.Protocol.Codecs.Buffer;

namespace Vint.Core.Server.Game.Protocol.Codecs.Impl;

public class EnumCodec(
    Type enumType
) : Codec {
    public override void Encode(ProtocolBuffer buffer, object value) {
        TypeCode? typeCode = (value as Enum)?.GetTypeCode();

        if (typeCode != TypeCode.Byte)
            throw new ArgumentException($"Enum TypeCode must be Byte. Current: {typeCode} ({value.GetType().Name})");

        buffer.Writer.Write((byte)value);
    }

    public override object Decode(ProtocolBuffer buffer) =>
        Enum.ToObject(enumType, buffer.Reader.ReadByte());
}
