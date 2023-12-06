using Vint.Core.Protocol.Codecs.Buffer;

namespace Vint.Core.Protocol.Codecs.Impl;

public class DateTimeOffsetCodec : Codec {
    public override void Encode(ProtocolBuffer buffer, object value) =>
        buffer.Writer.Write(((DateTimeOffset)value).ToUnixTimeMilliseconds());

    public override object Decode(ProtocolBuffer buffer) =>
        DateTimeOffset.FromUnixTimeMilliseconds(buffer.Reader.ReadInt64());
}