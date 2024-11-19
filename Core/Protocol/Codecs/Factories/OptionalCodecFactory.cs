using Vint.Core.Protocol.Codecs.Impl;
using Vint.Core.Protocol.Codecs.Info;

namespace Vint.Core.Protocol.Codecs.Factories;

public class OptionalCodecFactory : ICodecFactory {
    public ICodec? Create(Protocol protocol, CodecInfoWithAttributes codecInfoWithAttributes) {
        ICodecInfo codecInfo = codecInfoWithAttributes.CodecInfo;

        return codecInfo.Nullable
            ? new OptionalCodec(protocol.GetCodec(new CodecInfoWithAttributes(codecInfo.Type, false, codecInfo.Varied)))
            : null;
    }
}
