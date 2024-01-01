using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.User;

[ProtocolId(1470735489716)]
public class UserCountryComponent(
    string countryCode
) : IComponent {
    public string CountryCode { get; private set; } = countryCode;
}