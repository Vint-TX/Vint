using Vint.Core.Protocol.Codecs.Impl;
using Vint.Core.Protocol.Codecs.Info;

namespace Vint.Core.Protocol.Codecs.Factories;

public class EnumCodecFactory : ICodecFactory {
    Dictionary<Type, EnumCodec> Codecs { get; } = new();

    public ICodec? Create(Protocol protocol, CodecInfoWithAttributes codecInfoWithAttributes) {
        ICodecInfo codecInfo = codecInfoWithAttributes.CodecInfo;

        if (!codecInfo.Type.IsEnum) return null;

        Type type = codecInfo.Type;

        if (Codecs.TryGetValue(type, out EnumCodec? codec))
            return codec;

        codec = new EnumCodec(codecInfo.Type);
        Codecs[type] = codec;
        return codec;
    }
}
