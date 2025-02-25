﻿using Vint.Core.Server.Game.Protocol.Codecs.Buffer;

namespace Vint.Core.Server.Game.Protocol.Codecs.Impl;

public class ShortCodec : Codec {
    public override void Encode(ProtocolBuffer buffer, object value) => buffer.Writer.Write((short)value);

    public override object Decode(ProtocolBuffer buffer) => buffer.Reader.ReadInt16();
}
