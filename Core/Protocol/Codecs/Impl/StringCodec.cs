using System.Text;
using Vint.Core.Protocol.Codecs.Buffer;

namespace Vint.Core.Protocol.Codecs.Impl;

public class StringCodec : Codec {
    public override void Encode(ProtocolBuffer buffer, object value) {
        byte[] bytes = Encoding.UTF8.GetBytes((string)value);
        VarIntCodecHelper.Encode(buffer.Writer, bytes.Length);
        buffer.Writer.Write(bytes);
    }

    public override string Decode(ProtocolBuffer buffer) {
        int count = VarIntCodecHelper.Decode(buffer.Reader);
        return Encoding.UTF8.GetString(buffer.Reader.ReadBytes(count));
    }
}