using Vint.Core.Protocol.Attributes;
using Vint.Core.Protocol.Codecs.Impl;
using Vint.Core.Protocol.Codecs.Info;

namespace Vint.Core.Protocol.Codecs.Factories;

public class TimeSpanCodecFactory : ICodecFactory {
    public ICodec? Create(Protocol protocol, CodecInfoWithAttributes codecInfoWithAttributes) {
        ICodecInfo codecInfo = codecInfoWithAttributes.CodecInfo;

        if (!codecInfo.Type.IsAssignableTo(typeof(TimeSpan)))
            return null;

        ProtocolTimeKindAttribute timeKind = codecInfoWithAttributes.GetAttribute<ProtocolTimeKindAttribute>();
        return new TimeSpanCodec(timeKind.NumberType, timeKind.Kind);
    }
}
