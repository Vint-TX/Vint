using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Vint.Core.Server.API.Serialization;

public class RequiredFieldsContractResolver : DefaultContractResolver {
    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization) {
        JsonProperty property = base.CreateProperty(member, memberSerialization);

        property.Required = Required.AllowNull;
        return property;
    }
}
