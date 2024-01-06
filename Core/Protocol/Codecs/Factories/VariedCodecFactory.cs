using Vint.Core.Protocol.Codecs.Impl;

namespace Vint.Core.Protocol.Codecs.Factories;

public class VariedCodecFactory : ICodecFactory {
    public ICodec? Create(Protocol protocol, ICodecInfo codecInfo) {
        if (codecInfo is not TypeCodecInfo { Varied: true } typeCodecInfo) return null;

        return typeCodecInfo.Type == typeof(Type) ? new VariedTypeCodec() : new VariedStructCodec();
    }
}