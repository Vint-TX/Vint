using Vint.Core.ECS.Components;
using Vint.Core.ECS.Components.Group;
using Vint.Core.Protocol.Codecs.Buffer;
using Vint.Core.Protocol.Codecs.Info;
using Vint.Core.Utils;

namespace Vint.Core.Protocol.Codecs.Impl;

public class GroupComponentCodec : Codec {
    public override void Encode(ProtocolBuffer buffer, object value) {
        Protocol.GetCodec(typeof(long)).Encode(buffer, value.GetType().GetProtocolId());
        Protocol.GetCodec(typeof(long)).Encode(buffer, ((GroupComponent)value).Key);
    }

    public override GroupComponent Decode(ProtocolBuffer buffer) {
        long id = (long)Protocol.GetCodec(typeof(long)).Decode(buffer);
        long key = (long)Protocol.GetCodec(typeof(long)).Decode(buffer);
        Type type = Protocol.GetTypeById(id);

        return GroupComponentRegistry.FindOrCreateGroup(type, key);
    }
}
