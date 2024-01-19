using System.Collections;
using System.Reflection;
using Vint.Core.Protocol.Codecs.Buffer;

namespace Vint.Core.Protocol.Codecs.Impl;

public class HashSetCodec(
    Type hashSetType,
    ICodecInfo elementCodecInfo
) : Codec {
    public override void Encode(ProtocolBuffer buffer, object value) {
        int count = (int)hashSetType.GetProperty("Count")!.GetValue(value)!;
        VarIntCodecHelper.Encode(buffer.Writer, count);

        if (count <= 0) return;

        ICodec elementCodec = Protocol.GetCodec(elementCodecInfo);
        IEnumerator enumerator = ((IEnumerable)value).GetEnumerator();

        try {
            while (enumerator.MoveNext())
                elementCodec.Encode(buffer, enumerator.Current!);
        } finally {
            (enumerator as IDisposable)?.Dispose();
        }
    }

    public override object Decode(ProtocolBuffer buffer) {
        int count = VarIntCodecHelper.Decode(buffer.Reader);
        object hashSet = Activator.CreateInstance(hashSetType)!;

        if (count <= 0) return hashSet;

        ICodec elementCodec = Protocol.GetCodec(elementCodecInfo);
        MethodInfo addMethod = hashSetType.GetMethod("Add")!;

        for (int i = 0; i < count; i++)
            addMethod.Invoke(hashSet, [elementCodec.Decode(buffer)]);

        return hashSet;
    }
}