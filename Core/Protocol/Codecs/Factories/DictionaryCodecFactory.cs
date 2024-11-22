using Vint.Core.Protocol.Codecs.Impl;
using Vint.Core.Utils;

namespace Vint.Core.Protocol.Codecs.Factories;

public class DictionaryCodecFactory : ICodecFactory {
    public ICodec? Create(Protocol protocol, ICodecInfo codecInfo) {
        if (codecInfo is not ITypeCodecInfo typeCodecInfo ||
            !typeCodecInfo.Type.IsDictionary())
            return null;

        Type[] arguments = typeCodecInfo.Type.GenericTypeArguments;

        return new DictionaryCodec(new TypeCodecInfo(arguments[0]), new TypeCodecInfo(arguments[1]));
    }
}
