using Vint.Core.Protocol.Codecs.Impl;

namespace Vint.Core.Protocol.Codecs.Factories;

public class EnumCodecFactory : ICodecFactory {
    public ICodec? Create(Protocol protocol, ICodecInfo codecInfo) {
        if (codecInfo is not TypeCodecInfo typeCodecInfo || !typeCodecInfo.Type.IsEnum) return null;

        return new EnumCodec(typeCodecInfo.Type.GetEnumUnderlyingType());
    }
}