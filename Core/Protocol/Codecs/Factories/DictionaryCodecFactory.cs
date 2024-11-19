using Vint.Core.Protocol.Attributes;
using Vint.Core.Protocol.Codecs.Impl;
using Vint.Core.Protocol.Codecs.Info;
using Vint.Core.Utils;

namespace Vint.Core.Protocol.Codecs.Factories;

public class DictionaryCodecFactory : ICodecFactory {
    public ICodec? Create(Protocol protocol, CodecInfoWithAttributes codecInfoWithAttributes) {
        ICodecInfo codecInfo = codecInfoWithAttributes.CodecInfo;

        if (!codecInfo.Type.IsDictionary())
            return null;

        Type[] arguments = codecInfo.Type.GenericTypeArguments;

        return new DictionaryCodec(
            new CodecInfoWithAttributes(arguments[0]),
            new CodecInfoWithAttributes(arguments[1]));
    }
}
