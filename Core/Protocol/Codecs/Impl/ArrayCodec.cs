using Vint.Core.Protocol.Codecs.Buffer;
using Vint.Core.Protocol.Codecs.Helpers;
using Vint.Core.Protocol.Codecs.Info;

namespace Vint.Core.Protocol.Codecs.Impl;

public class ArrayCodec(
    CodecInfoWithAttributes elementCodecInfo
) : Codec {
    public override void Encode(ProtocolBuffer buffer, object value) {
        if (value is not Array array)
            throw new ArgumentException("Value must be array");

        ICodec elementCodec = Protocol.GetCodec(elementCodecInfo);
        VarIntCodecHelper.Encode(buffer.Writer, array.Length);

        foreach (object element in array)
            elementCodec.Encode(buffer, element);
    }

    public override object Decode(ProtocolBuffer buffer) {
        int count = VarIntCodecHelper.Decode(buffer.Reader);
        ICodec elementCodec = Protocol.GetCodec(elementCodecInfo);
        Array array = Array.CreateInstance(elementCodecInfo.CodecInfo.Type, count);

        for (int i = 0; i < count; i++)
            array.SetValue(elementCodec.Decode(buffer), i);

        return array;
    }
}
