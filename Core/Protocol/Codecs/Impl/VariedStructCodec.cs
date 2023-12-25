using Vint.Core.Protocol.Codecs.Buffer;
using Vint.Core.Utils;

namespace Vint.Core.Protocol.Codecs.Impl;

public class VariedStructCodec : Codec {
    public override void Encode(ProtocolBuffer buffer, object value) {
        Type type = value.GetType();
        buffer.Writer.Write(type.GetProtocolId().Id);

        Protocol.GetCodec(new TypeCodecInfo(type)).Encode(buffer, value);
    }

    public override object Decode(ProtocolBuffer buffer) {
        long id = buffer.Reader.ReadInt64();
        Type type = Protocol.GetTypeById(id);

        ProtocolBuffer inner = new(new OptionalMap(), buffer.Connection);

        if (!inner.Unwrap(buffer.Reader)) throw new InvalidOperationException();

        return Protocol.GetCodec(new TypeCodecInfo(type)).Decode(inner);
    }
}