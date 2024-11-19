using Vint.Core.Protocol.Codecs.Buffer;
using Vint.Core.Protocol.Codecs.Info;
using Vint.Core.Utils;

namespace Vint.Core.Protocol.Codecs.Impl;

public class VariedStructCodec : Codec {
    public override void Encode(ProtocolBuffer buffer, object value) {
        Type type = value.GetType();
        buffer.Writer.Write(type.GetProtocolId());

        Protocol.GetCodec(type).Encode(buffer, value);
    }

    public override object Decode(ProtocolBuffer buffer) {
        long id = buffer.Reader.ReadInt64();

        ProtocolBuffer inner = new(new OptionalMap(), buffer.Connection);

        if (!inner.Unwrap(buffer.Reader))
            throw new InvalidOperationException();

        return Protocol.GetCodec(id).Decode(inner);
    }
}
