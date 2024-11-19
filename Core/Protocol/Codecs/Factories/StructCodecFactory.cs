using System.Reflection;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Protocol.Codecs.Impl;
using Vint.Core.Protocol.Codecs.Info;
using Vint.Core.Utils;

namespace Vint.Core.Protocol.Codecs.Factories;

public class StructCodecFactory : ICodecFactory {
    public ICodec Create(Protocol protocol, CodecInfoWithAttributes codecInfoWithAttributes) {
        ICodecInfo codecInfo = codecInfoWithAttributes.CodecInfo;

        List<PropertyDescription> properties = GetSortedProperties(codecInfo.Type)
            .Select(property => new PropertyDescription(property,
                new CodecInfoWithAttributes(property.PropertyType,
                    property.IsNullable(),
                    property.GetCustomAttribute<ProtocolVariedAttribute>() != null,
                    property.GetCustomAttributes())))
            .ToList();

        return new StructCodec(codecInfo.Type, properties);
    }

    static IOrderedEnumerable<PropertyInfo> GetSortedProperties(Type type) => type.GetProperties()
        .Where(property => property.GetCustomAttribute<ProtocolIgnoreAttribute>() == null)
        .OrderBy(property => property.GetCustomAttribute<ProtocolPositionAttribute>()?.Position)
        .ThenBy(property => property.GetCustomAttribute<ProtocolNameAttribute>()?.Name ?? property.Name, StringComparer.Ordinal);
}
