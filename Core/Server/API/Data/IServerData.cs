using JetBrains.Annotations;

namespace Vint.Core.Server.API.Data;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature, ImplicitUseTargetFlags.WithInheritors)]
public interface IServerData : IData {
    Task<IClientData> Process();
}
