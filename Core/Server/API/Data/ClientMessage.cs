using JetBrains.Annotations;

namespace Vint.Core.Server.API.Data;

[UsedImplicitly(ImplicitUseKindFlags.Access, ImplicitUseTargetFlags.Members)]
record ClientMessage(
    int Id,
    int RequestId,
    IClientDTO Data
);
