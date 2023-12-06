using Vint.Core.Protocol.Codecs.Buffer;

namespace Vint.Core.Protocol.Codecs;

public interface ICodec {
    public Protocol Protocol { get; }

    public void Init(Protocol protocol);

    public void Encode(ProtocolBuffer buffer, object value);

    public object Decode(ProtocolBuffer buffer);
}