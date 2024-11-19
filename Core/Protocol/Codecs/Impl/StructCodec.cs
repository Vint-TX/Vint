using System.Reflection;
using System.Runtime.CompilerServices;
using Vint.Core.Protocol.Codecs.Buffer;
using Vint.Core.Protocol.Codecs.Info;

namespace Vint.Core.Protocol.Codecs.Impl;

public class StructCodec(
    Type type,
    List<PropertyDescription> properties
) : Codec {
    public override void Encode(ProtocolBuffer buffer, object value) {
        foreach ((PropertyInfo property, CodecInfoWithAttributes codecInfo) in properties) {
            object? item = property.GetValue(value);

            Protocol.GetCodec(codecInfo).Encode(buffer, item!);
        }
    }

    public override object Decode(ProtocolBuffer buffer) {
        object value = RuntimeHelpers.GetUninitializedObject(type);

        foreach ((PropertyInfo property, CodecInfoWithAttributes codecInfo) in properties) {
            object item = Protocol.GetCodec(codecInfo).Decode(buffer);

            property.SetValue(value, item, null);
        }

        return value;
    }
}

public readonly record struct PropertyDescription(
    PropertyInfo Property,
    CodecInfoWithAttributes CodecInfoWithAttributes
) {
    ICodecInfo CodecInfo => CodecInfoWithAttributes.CodecInfo;
}
