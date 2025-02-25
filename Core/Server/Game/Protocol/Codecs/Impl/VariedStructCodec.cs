﻿using Vint.Core.Server.Game.Protocol.Codecs.Buffer;
using Vint.Core.Utils;

namespace Vint.Core.Server.Game.Protocol.Codecs.Impl;

public class VariedStructCodec : Codec {
    public override void Encode(ProtocolBuffer buffer, object value) {
        Type type = value.GetType();
        buffer.Writer.Write(type.GetProtocolId());

        Protocol
            .GetCodec(new TypeCodecInfo(type))
            .Encode(buffer, value);
    }

    public override object Decode(ProtocolBuffer buffer) {
        long id = buffer.Reader.ReadInt64();
        Type type = Protocol.GetTypeById(id);

        using ProtocolBuffer inner = ProtocolBuffer.Unwrap(buffer.Reader, buffer.Connection);

        return Protocol
            .GetCodec(new TypeCodecInfo(type))
            .Decode(inner);
    }
}
