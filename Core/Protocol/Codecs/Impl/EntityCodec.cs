using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Codecs.Buffer;

namespace Vint.Core.Protocol.Codecs.Impl;

public class EntityCodec : Codec {
    public override void Encode(ProtocolBuffer buffer, object value) =>
        Protocol.GetCodec(new TypeCodecInfo(typeof(long))).Encode(buffer, ((IEntity)value).Id);

    public override IEntity Decode(ProtocolBuffer buffer) {
        object id = Protocol.GetCodec(new TypeCodecInfo(typeof(long))).Decode(buffer);
        return EntityRegistry.Get((long)id);
    }
}
