using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Vint.Core.Server.Game.Protocol.Codecs.Buffer;

namespace Vint.Core.Server.Game.Protocol.Codecs.Impl;

public class StructCodec(
    Type type,
    List<PropertyRequest> properties
) : Codec {
    public override void Encode(ProtocolBuffer buffer, object value) {
        foreach ((PropertyInfo property, ICodecInfo codecInfo) in properties) {
            object? item = property.GetValue(value);

            Protocol
                .GetCodec(codecInfo)
                .Encode(buffer, item!);
        }
    }

    public override object Decode(ProtocolBuffer buffer) {
        IServiceScope serviceScope = buffer.CreateServiceScope();
        object value = ActivatorUtilities.CreateInstance(serviceScope.ServiceProvider, type);
        //object value = RuntimeHelpers.GetUninitializedObject(type);

        foreach ((PropertyInfo property, ICodecInfo codecInfo) in properties) {
            object item = Protocol
                .GetCodec(codecInfo)
                .Decode(buffer);

            property.SetValue(value, item, null);
        }

        return value;
    }
}

public readonly record struct PropertyRequest(
    PropertyInfo Property,
    ICodecInfo CodecInfo
);
