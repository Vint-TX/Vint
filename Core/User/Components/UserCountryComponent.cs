using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.User.Components;

[ProtocolId(1470735489716)]
public class UserCountryComponent(
    string countryCode
) : PrivateComponent {
    public string CountryCode { get; set; } = countryCode;
}
