using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Vint.Core.Server.Game.Protocol.Codecs.Buffer;

namespace Vint.Core.Server.Game.Protocol.Codecs.Impl;

public class StructCodec(
    Type type,
    List<PropertyRequest> properties
) : Codec {
    public override void Encode(ProtocolBuffer buffer, object obj) {
        foreach ((PropertyInfo property, ICodecInfo codecInfo) in properties) {
            object? value = property.GetValue(obj);

            Protocol.GetCodec(codecInfo).Encode(buffer, value!);
        }
    }

    public override object Decode(ProtocolBuffer buffer) {
        IServiceScope serviceScope = buffer.CreateServiceScope();
        object obj = ActivatorUtilities.CreateInstance(serviceScope.ServiceProvider, type);

        foreach ((PropertyInfo property, ICodecInfo codecInfo) in properties) {
            object value = Protocol.GetCodec(codecInfo).Decode(buffer);

            property.SetValue(obj, value, null);
        }

        return obj;
    }
}

public readonly record struct PropertyRequest(
    PropertyInfo Property,
    ICodecInfo CodecInfo
);
