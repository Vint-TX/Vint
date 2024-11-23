using Vint.Core.Server.Game.Protocol.Codecs.Impl;

namespace Vint.Core.Server.Game.Protocol.Codecs.Factories;

public class OptionalCodecFactory : ICodecFactory {
    public ICodec? Create(Protocol protocol, ICodecInfo codecInfo) {
        if (codecInfo is not ITypeCodecInfo { Nullable: true } typeCodecInfo) return null;

        return new OptionalCodec(protocol.GetCodec(new TypeCodecInfo(typeCodecInfo.Type, false, typeCodecInfo.Varied, typeCodecInfo.Attributes)));
    }
}
