using System.Collections;
using Vint.Core.Protocol.Codecs.Buffer;

namespace Vint.Core.Protocol.Codecs.Impl;

public class HashSetCodec(ICodecInfo elementCodecInfo) : Codec {
    public override void Encode(ProtocolBuffer buffer, object value) {
        IList hashSetList = (IList)typeof(Enumerable)
            .GetMethod("ToList")!
            .MakeGenericMethod(value.GetType().GenericTypeArguments[0])
            .Invoke(value, [value])!;

        ICodec elementCodec = Protocol.GetCodec(elementCodecInfo);
        VarIntCodecHelper.Encode(buffer.Writer, hashSetList.Count);

        foreach (object element in hashSetList)
            elementCodec.Encode(buffer, element);
    }

    public override HashSet<object> Decode(ProtocolBuffer buffer) {
        ICodec elementCodec = Protocol.GetCodec(elementCodecInfo);
        int count = VarIntCodecHelper.Decode(buffer.Reader);
        HashSet<object> hashSet = new(count);

        while (hashSet.Count < count)
            hashSet.Add(elementCodec.Decode(buffer));

        return hashSet;
    }
}