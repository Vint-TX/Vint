using Vint.Core.Protocol.Attributes;
using Vint.Core.Protocol.Codecs.Impl;
using Vint.Core.Protocol.Codecs.Info;

namespace Vint.Core.Protocol.Codecs.Factories;

public class ArrayCodecFactory : ICodecFactory {
    public ICodec? Create(Protocol protocol, CodecInfoWithAttributes codecInfoWithAttributes) {
        ICodecInfo codecInfo = codecInfoWithAttributes.CodecInfo;

        if (!codecInfo.Type.IsArray)
            return null;

        ProtocolCollectionAttribute protocolCollection = codecInfoWithAttributes.HasAttribute<ProtocolCollectionAttribute>()
            ? codecInfoWithAttributes.GetAttribute<ProtocolCollectionAttribute>()
            : new ProtocolCollectionAttribute();

        return new ArrayCodec(
            new CodecInfoWithAttributes(
                codecInfo.Type.GetElementType()!,
                protocolCollection.Nullable,
                protocolCollection.Varied));
    }
}
