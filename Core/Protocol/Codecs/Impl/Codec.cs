using Vint.Core.Protocol.Codecs.Buffer;

namespace Vint.Core.Protocol.Codecs.Impl;

public abstract class Codec : ICodec {
    public Protocol Protocol { get; private set; } = null!;

    public virtual void Init(Protocol protocol) => Protocol = protocol;

    public abstract void Encode(ProtocolBuffer buffer, object value);

    public abstract object Decode(ProtocolBuffer buffer);
}