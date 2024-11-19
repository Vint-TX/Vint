﻿using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Codecs.Buffer;
using Vint.Core.Protocol.Codecs.Info;

namespace Vint.Core.Protocol.Codecs.Impl;

public class EntityCodec : Codec {
    public override void Encode(ProtocolBuffer buffer, object value) =>
        Protocol.GetCodec(typeof(long)).Encode(buffer, ((IEntity)value).Id);

    public override object Decode(ProtocolBuffer buffer) {
        long id = (long)Protocol.GetCodec(typeof(long)).Decode(buffer);

        return buffer.GetSharedEntity(id) ?? (EntityRegistry.TryGetTemp(id, out IEntity? entity) ? entity : EntityRegistry.Get(id));
    }
}
