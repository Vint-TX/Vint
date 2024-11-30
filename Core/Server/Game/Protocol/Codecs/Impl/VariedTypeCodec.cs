using Vint.Core.Server.Game.Protocol.Codecs.Buffer;
using Vint.Core.Utils;

namespace Vint.Core.Server.Game.Protocol.Codecs.Impl;

public class VariedTypeCodec : Codec {
    public override void Encode(ProtocolBuffer buffer, object value) {
        if (value is not Type type) throw new ArgumentException("Value must be Type", nameof(value));

        buffer.Writer.Write(type.GetProtocolId());
    }

    public override object Decode(ProtocolBuffer buffer) {
        long id = buffer.Reader.ReadInt64();
        return Protocol.GetTypeById(id);
    }
}
