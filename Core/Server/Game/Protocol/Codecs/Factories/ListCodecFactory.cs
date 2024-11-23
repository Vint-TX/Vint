using Vint.Core.Server.Game.Protocol.Attributes;
using Vint.Core.Server.Game.Protocol.Codecs.Impl;
using Vint.Core.Utils;

namespace Vint.Core.Server.Game.Protocol.Codecs.Factories;

public class ListCodecFactory : ICodecFactory {
    public ICodec? Create(Protocol protocol, ICodecInfo codecInfo) {
        if (codecInfo is not ITypeCodecInfo typeCodecInfo ||
            !typeCodecInfo.Type.IsList())
            return null;

        ProtocolCollectionAttribute protocolCollection = typeCodecInfo
                                                             .Attributes
                                                             .OfType<ProtocolCollectionAttribute>()
                                                             .FirstOrDefault() ??
                                                         ProtocolCollectionAttribute.Default;

        return new ListCodec(typeCodecInfo.Type,
            new TypeCodecInfo(typeCodecInfo.Type.GenericTypeArguments.Single(), protocolCollection.Nullable, protocolCollection.Varied));
    }
}
