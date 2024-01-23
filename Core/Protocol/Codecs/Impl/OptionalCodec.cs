using Vint.Core.Protocol.Codecs.Buffer;

namespace Vint.Core.Protocol.Codecs.Impl;

public class OptionalCodec : Codec {
    public OptionalCodec(ICodec codec) {
        if (codec is OptionalCodec)
            throw new NotSupportedException("OptionalCodec can not be used as inner codec for OptionalCodec");

        Codec = codec;
    }

    ICodec Codec { get; }

    public override void Init(Protocol protocol) {
        base.Init(protocol);
        Codec.Init(protocol);
    }

    public override void Encode(ProtocolBuffer buffer, object? value) {
        buffer.OptionalMap.Add(value == null);
        if (value == null) return;

        Type? underlyingType = Nullable.GetUnderlyingType(value.GetType());

        if (underlyingType != null)
            value = Convert.ChangeType(value, underlyingType);

        Codec.Encode(buffer, value);
    }

    public override object Decode(ProtocolBuffer buffer) => buffer.OptionalMap.Get()
                                                                ? null!
                                                                : Codec.Decode(buffer);
}