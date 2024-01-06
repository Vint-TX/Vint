using Vint.Core.Protocol.Codecs.Buffer;
using Vint.Core.Utils;

namespace Vint.Core.Protocol.Codecs.Impl;

public class VariedTypeCodec : Codec {
    public override void Encode(ProtocolBuffer buffer, object value) {
        if (value is not Type type) throw new ArgumentException("Value must be Type", nameof(value));

        Protocol.GetCodec(new TypeCodecInfo(typeof(long))).Encode(buffer, type.GetProtocolId().Id);
    }

    public override object Decode(ProtocolBuffer buffer) {
        object id = Protocol.GetCodec(new TypeCodecInfo(typeof(long))).Decode(buffer);
        return Protocol.GetTypeById((long)id);
    }
}