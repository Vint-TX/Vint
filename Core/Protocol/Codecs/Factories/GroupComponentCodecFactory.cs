using Vint.Core.ECS.Components.Group;
using Vint.Core.Protocol.Codecs.Impl;
using Vint.Core.Protocol.Codecs.Info;

namespace Vint.Core.Protocol.Codecs.Factories;

public class GroupComponentCodecFactory : ICodecFactory {
    public ICodec? Create(Protocol protocol, CodecInfoWithAttributes codecInfoWithAttributes) =>
        codecInfoWithAttributes.CodecInfo.Type.IsAssignableTo(typeof(GroupComponent))
            ? new GroupComponentCodec()
            : null;
}
