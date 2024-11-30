using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Codecs.Buffer;

namespace Vint.Core.Server.Game.Protocol.Codecs.Impl;

public class EntityCodec : Codec {
    public override void Encode(ProtocolBuffer buffer, object value) =>
        buffer.Writer.Write(((IEntity)value).Id);

    public override object Decode(ProtocolBuffer buffer) {
        long id = buffer.Reader.ReadInt64();

        return buffer.GetSharedEntity(id) ??
               (EntityRegistry.TryGetTemp(id, out IEntity? entity)
                   ? entity
                   : EntityRegistry.Get(id));
    }
}
