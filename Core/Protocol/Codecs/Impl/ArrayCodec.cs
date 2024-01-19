using Vint.Core.Protocol.Codecs.Buffer;

namespace Vint.Core.Protocol.Codecs.Impl;

public class ArrayCodec(
    ICodecInfo elementCodecInfo
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
        if (elementCodecInfo is not ITypeCodecInfo elementTypeCodecInfo)
            throw new NotSupportedException("CodecInfo must implement ITypeCodecInfo");

        int count = VarIntCodecHelper.Decode(buffer.Reader);
        ICodec elementCodec = Protocol.GetCodec(elementCodecInfo);
        Array array = Array.CreateInstance(elementTypeCodecInfo.Type, count);

        for (int i = 0; i < count; i++)
            array.SetValue(elementCodec.Decode(buffer), i);

        return array;
    }
}