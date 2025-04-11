using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;

namespace Vint.Core.Server.Common.Serialization;

public class DependencyInjectionContractResolver(
    IServiceProvider serviceProvider
) : DefaultContractResolver {
    protected override JsonObjectContract CreateObjectContract(Type objectType) {
        JsonObjectContract contract = base.CreateObjectContract(objectType);
        contract.DefaultCreator = () => ActivatorUtilities.CreateInstance(serviceProvider, objectType);
        return contract;
    }
}
