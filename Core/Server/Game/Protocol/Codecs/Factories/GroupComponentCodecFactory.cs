using Vint.Core.ECS.Components.Group;
using Vint.Core.Server.Game.Protocol.Codecs.Impl;

namespace Vint.Core.Server.Game.Protocol.Codecs.Factories;

public class GroupComponentCodecFactory : ICodecFactory {
    public ICodec? Create(Protocol protocol, ICodecInfo codecInfo) {
        if (codecInfo is not ITypeCodecInfo typeCodecInfo ||
            !typeCodecInfo.Type.IsAssignableTo(typeof(GroupComponent)))
            return null;

        return new GroupComponentCodec();
    }
}
