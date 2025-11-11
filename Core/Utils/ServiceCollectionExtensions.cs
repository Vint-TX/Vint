using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Vint.Core.Utils;

public static class ServiceCollectionExtensions {
    extension(IServiceCollection serviceCollection) {
        public IServiceCollection AddHostedSingletonService<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors), MeansImplicitUse(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)] T>() where T : class, IHostedService =>
            serviceCollection
                .AddSingleton<T>()
                .AddHostedService<T>(provider => provider.GetRequiredService<T>());

        public IServiceCollection AddHostedSingletonService<T>(Func<IServiceProvider, T> implementationFactory) where T : class, IHostedService =>
            serviceCollection
                .AddSingleton(implementationFactory)
                .AddHostedService<T>(provider => provider.GetRequiredService<T>());

        public IServiceCollection AddHostedSingletonService<T>(T implementationInstance) where T : class, IHostedService =>
            serviceCollection
                .AddSingleton(implementationInstance)
                .AddHostedService<T>(provider => provider.GetRequiredService<T>());
    }
}
