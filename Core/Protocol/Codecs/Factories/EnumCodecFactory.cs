using Vint.Core.Protocol.Codecs.Impl;

namespace Vint.Core.Protocol.Codecs.Factories;

public class EnumCodecFactory : ICodecFactory {
    Dictionary<Type, EnumCodec> Codecs { get; } = new();

    public ICodec? Create(Protocol protocol, ICodecInfo codecInfo) {
        if (codecInfo is not TypeCodecInfo typeCodecInfo || !typeCodecInfo.Type.IsEnum) return null;

        Type type = typeCodecInfo.Type;

        if (Codecs.TryGetValue(type, out EnumCodec? codec))
            return codec;

        codec = new EnumCodec(typeCodecInfo.Type);
        Codecs[type] = codec;
        return codec;
    }
}