using System.Collections.Concurrent;
using Vint.Core.Server.Game.Protocol.Codecs.Impl;

namespace Vint.Core.Server.Game.Protocol.Codecs.Factories;

public class EnumCodecFactory : ICodecFactory {
    ConcurrentDictionary<Type, EnumCodec> Codecs { get; } = new();

    public ICodec? Create(Protocol protocol, ICodecInfo codecInfo) {
        if (codecInfo is not TypeCodecInfo typeCodecInfo || !typeCodecInfo.Type.IsEnum)
            return null;

        return Codecs.GetOrAdd(typeCodecInfo.Type, type => new EnumCodec(type));
    }
}
