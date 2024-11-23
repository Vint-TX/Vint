using Vint.Core.Server.Game.Protocol.Codecs.Buffer;

namespace Vint.Core.Server.Game.Protocol.Codecs;

public interface ICodec {
    Protocol Protocol { get; }

    void Init(Protocol protocol);

    void Encode(ProtocolBuffer buffer, object value);

    object Decode(ProtocolBuffer buffer);
}
