using Vint.Core.ECS.Components.Group;
using Vint.Core.Protocol.Codecs.Buffer;
using Vint.Core.Utils;

namespace Vint.Core.Protocol.Codecs.Impl;

public class GroupComponentCodec : Codec {
    public override void Encode(ProtocolBuffer buffer, object value) {
        Protocol.GetCodec(new TypeCodecInfo(typeof(long))).Encode(buffer, value.GetType().GetProtocolId().Id);
        Protocol.GetCodec(new TypeCodecInfo(typeof(long))).Encode(buffer, ((GroupComponent)value).Key);
    }

    public override GroupComponent Decode(ProtocolBuffer buffer) {
        object id = Protocol.GetCodec(new TypeCodecInfo(typeof(long))).Decode(buffer);
        object key = Protocol.GetCodec(new TypeCodecInfo(typeof(long))).Decode(buffer);
        Type type = Protocol.GetTypeById((long)id);

        return (type.GetConstructors()
                    .Single(c =>
                        c.GetParameters().Single().ParameterType == typeof(long))
                    .Invoke(new[] { key }) as GroupComponent)!;
    }
}