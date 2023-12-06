using System.Collections;
using Vint.Core.Protocol.Codecs.Buffer;

namespace Vint.Core.Protocol.Codecs.Impl;

public class DictionaryCodec(
    ICodecInfo keyCodecInfo,
    ICodecInfo valueCodecInfo
) : Codec {
    public override void Encode(ProtocolBuffer buffer, object value) {
        if (value is not IDictionary dict)
            throw new ArgumentException("Value must be dictionary");

        ICodec keyCodec = Protocol.GetCodec(keyCodecInfo);
        ICodec valueCodec = Protocol.GetCodec(valueCodecInfo);

        VarIntCodecHelper.Encode(buffer.Writer, dict.Count);

        foreach (DictionaryEntry entry in dict) {
            keyCodec.Encode(buffer, entry.Key);
            valueCodec.Encode(buffer, entry.Value!);
        }
    }

    public override Dictionary<object, object> Decode(ProtocolBuffer buffer) {
        ICodec keyCodec = Protocol.GetCodec(keyCodecInfo);
        ICodec valueCodec = Protocol.GetCodec(valueCodecInfo);
        int count = VarIntCodecHelper.Decode(buffer.Reader);
        Dictionary<object, object> dictionary = new(count);

        while (dictionary.Count < count) {
            object k = keyCodec.Decode(buffer);
            object v = valueCodec.Decode(buffer);

            dictionary[k] = v;
        }

        return dictionary;
    }
}