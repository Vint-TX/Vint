using Vint.Core.Protocol.Attributes;
using Vint.Core.Protocol.Codecs.Impl;
using Vint.Core.Utils;

namespace Vint.Core.Protocol.Codecs.Factories;

public class HashSetCodecFactory : ICodecFactory {
    public ICodec? Create(Protocol protocol, ICodecInfo codecInfo) {
        if (codecInfo is not ITypeCodecInfo typeCodecInfo ||
            !typeCodecInfo.Type.IsHashSet())
            return null;

        ProtocolCollectionAttribute protocolCollection = typeCodecInfo.Attributes
                                                             .OfType<ProtocolCollectionAttribute>()
                                                             .FirstOrDefault() ??
                                                         ProtocolCollectionAttribute.Default;

        return new HashSetCodec(
            typeCodecInfo.Type,
            new TypeCodecInfo(
                typeCodecInfo.Type.GenericTypeArguments.Single(),
                protocolCollection.Nullable,
                protocolCollection.Varied));
    }
}