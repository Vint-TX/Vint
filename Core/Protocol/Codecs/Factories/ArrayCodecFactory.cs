using Vint.Core.Protocol.Attributes;
using Vint.Core.Protocol.Codecs.Impl;

namespace Vint.Core.Protocol.Codecs.Factories;

public class ArrayCodecFactory : ICodecFactory {
    public ICodec? Create(Protocol protocol, ICodecInfo codecInfo) {
        if (codecInfo is not ITypeCodecInfo typeCodecInfo ||
            !typeCodecInfo.Type.IsArray)
            return null;

        ProtocolCollectionAttribute protocolCollection = typeCodecInfo.Attributes
                                                             .OfType<ProtocolCollectionAttribute>()
                                                             .FirstOrDefault() ??
                                                         ProtocolCollectionAttribute.Default;

        return new ArrayCodec(
            new TypeCodecInfo(
                typeCodecInfo.Type.GetElementType()!,
                protocolCollection.Nullable,
                protocolCollection.Varied));
    }
}