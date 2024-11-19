using Vint.Core.Protocol.Attributes;
using Vint.Core.Protocol.Codecs.Impl;
using Vint.Core.Protocol.Codecs.Info;
using Vint.Core.Utils;

namespace Vint.Core.Protocol.Codecs.Factories;

public class HashSetCodecFactory : ICodecFactory {
    public ICodec? Create(Protocol protocol, CodecInfoWithAttributes codecInfoWithAttributes) {
        ICodecInfo codecInfo = codecInfoWithAttributes.CodecInfo;

        if (!codecInfo.Type.IsHashSet())
            return null;

        ProtocolCollectionAttribute protocolCollection = codecInfoWithAttributes.HasAttribute<ProtocolCollectionAttribute>()
            ? codecInfoWithAttributes.GetAttribute<ProtocolCollectionAttribute>()
            : new ProtocolCollectionAttribute();

        return new HashSetCodec(
            codecInfo.Type,
            new CodecInfoWithAttributes(
                codecInfo.Type.GenericTypeArguments.Single(),
                protocolCollection.Nullable,
                protocolCollection.Varied));
    }
}
