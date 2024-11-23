using System.Reflection;
using Vint.Core.Server.Game.Protocol.Attributes;
using Vint.Core.Server.Game.Protocol.Codecs.Impl;
using Vint.Core.Utils;

namespace Vint.Core.Server.Game.Protocol.Codecs.Factories;

public class StructCodecFactory : ICodecFactory {
    public ICodec? Create(Protocol protocol, ICodecInfo codecInfo) {
        if (codecInfo is not ITypeCodecInfo typeCodecInfo) return null;

        List<PropertyRequest> properties = GetSortedProperties(typeCodecInfo.Type)
            .Select(property => new PropertyRequest(property,
                new TypeCodecInfo(property.PropertyType,
                    property.IsNullable(),
                    property.GetCustomAttribute<ProtocolVariedAttribute>() != null,
                    property
                        .GetCustomAttributes<ProtocolCollectionAttribute>()
                        .Cast<Attribute>()
                        .ToHashSet())))
            .ToList();

        return new StructCodec(typeCodecInfo.Type, properties);
    }

    static IOrderedEnumerable<PropertyInfo> GetSortedProperties(Type type) => type
        .GetProperties()
        .Where(property => property.GetCustomAttribute<ProtocolIgnoreAttribute>() == null)
        .OrderBy(property => property.GetCustomAttribute<ProtocolPositionAttribute>()
            ?.Position)
        .ThenBy(property => property.GetCustomAttribute<ProtocolNameAttribute>()
                                ?.Name ??
                            property.Name,
            StringComparer.Ordinal);
}
