using Vint.Core.Protocol.Codecs.Buffer;

namespace Vint.Core.Protocol.Codecs;

public interface ICodec {
    Protocol Protocol { get; }

    void Init(Protocol protocol);

    void Encode(ProtocolBuffer buffer, object value);

    object Decode(ProtocolBuffer buffer);
}
