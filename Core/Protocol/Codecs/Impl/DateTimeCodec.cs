using Vint.Core.Protocol.Codecs.Buffer;

namespace Vint.Core.Protocol.Codecs.Impl;

public class DateTimeCodec : Codec {
    public override void Encode(ProtocolBuffer buffer, object value) {
        DateTime dateTime = (DateTime)value;
        buffer.Writer.Write(new DateTimeOffset(dateTime).ToUnixTimeMilliseconds());
    }

    public override object Decode(ProtocolBuffer buffer) =>
        DateTimeOffset.FromUnixTimeMilliseconds(buffer.Reader.ReadInt64()).UtcDateTime;
}