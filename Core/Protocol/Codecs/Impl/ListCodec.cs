using System.Collections;
using Vint.Core.Protocol.Codecs.Buffer;
using Vint.Core.Protocol.Codecs.Helpers;
using Vint.Core.Protocol.Codecs.Info;

namespace Vint.Core.Protocol.Codecs.Impl;

public class ListCodec(
    Type listType,
    CodecInfoWithAttributes elementCodecInfo
) : Codec {
    public override void Encode(ProtocolBuffer buffer, object value) {
        if (value is not IList list)
            throw new ArgumentException("Value must be list");

        ICodec elementCodec = Protocol.GetCodec(elementCodecInfo);
        VarIntCodecHelper.Encode(buffer.Writer, list.Count);

        foreach (object element in list)
            elementCodec.Encode(buffer, element);
    }

    public override object Decode(ProtocolBuffer buffer) {
        IList list = (IList)Activator.CreateInstance(listType)!;
        ICodec elementCodec = Protocol.GetCodec(elementCodecInfo);
        int count = VarIntCodecHelper.Decode(buffer.Reader);

        while (list.Count < count)
            list.Add(elementCodec.Decode(buffer));

        return list;
    }
}
