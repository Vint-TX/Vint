using Vint.Core.Server.Game.Protocol.Attributes;
using Vint.Core.Server.Game.Protocol.Codecs.Impl;

namespace Vint.Core.Server.Game.Protocol.Codecs.Factories;

public class ArrayCodecFactory : ICodecFactory {
    public ICodec? Create(Protocol protocol, ICodecInfo codecInfo) {
        if (codecInfo is not ITypeCodecInfo typeCodecInfo ||
            !typeCodecInfo.Type.IsArray)
            return null;

        ProtocolCollectionAttribute protocolCollection = typeCodecInfo
                                                             .Attributes
                                                             .OfType<ProtocolCollectionAttribute>()
                                                             .FirstOrDefault() ??
                                                         ProtocolCollectionAttribute.Default;

        return new ArrayCodec(new TypeCodecInfo(typeCodecInfo.Type.GetElementType()!, protocolCollection.Nullable, protocolCollection.Varied));
    }
}
