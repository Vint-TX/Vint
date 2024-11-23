using Vint.Core.Server.Game.Protocol.Codecs.Buffer;

namespace Vint.Core.Server.Game.Protocol.Codecs.Impl;

public abstract class Codec : ICodec {
    public Protocol Protocol { get; private set; } = null!;

    public virtual void Init(Protocol protocol) => Protocol = protocol;

    public abstract void Encode(ProtocolBuffer buffer, object value);

    public abstract object Decode(ProtocolBuffer buffer);

    public override string ToString() => GetType()
        .Name;
}
