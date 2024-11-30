using Vint.Core.ECS.Components.Group;
using Vint.Core.Server.Game.Protocol.Codecs.Buffer;
using Vint.Core.Utils;

namespace Vint.Core.Server.Game.Protocol.Codecs.Impl;

public class GroupComponentCodec : Codec {
    public override void Encode(ProtocolBuffer buffer, object value) {
        buffer.Writer.Write(value.GetType().GetProtocolId());
        buffer.Writer.Write(((GroupComponent)value).Key);
    }

    public override object Decode(ProtocolBuffer buffer) {
        long id = buffer.Reader.ReadInt64();
        long key = buffer.Reader.ReadInt64();

        Type type = Protocol.GetTypeById(id);

        return type
            .GetConstructors()
            .Single(c => c.GetParameters().Single().ParameterType == typeof(long))
            .Invoke([key]);
    }
}
