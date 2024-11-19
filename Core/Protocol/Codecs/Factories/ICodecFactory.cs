using Vint.Core.Protocol.Codecs.Info;

namespace Vint.Core.Protocol.Codecs.Factories;

public interface ICodecFactory {
    ICodec? Create(Protocol protocol, CodecInfoWithAttributes codecInfoWithAttributes);
}
