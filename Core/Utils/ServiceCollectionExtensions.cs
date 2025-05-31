using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Vint.Core.Utils;

public static class ServiceCollectionExtensions {
    public static IServiceCollection AddHostedSingletonService<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors), MeansImplicitUse(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)] T>(this IServiceCollection serviceCollection) where T : class, IHostedService =>
        serviceCollection
            .AddSingleton<T>()
            .AddHostedService<T>(provider => provider.GetRequiredService<T>());

    public static IServiceCollection AddHostedSingletonService<T>(this IServiceCollection serviceCollection, Func<IServiceProvider, T> implementationFactory) where T : class, IHostedService =>
        serviceCollection
            .AddSingleton(implementationFactory)
            .AddHostedService<T>(provider => provider.GetRequiredService<T>());

    public static IServiceCollection AddHostedSingletonService<T>(this IServiceCollection serviceCollection, T implementationInstance) where T : class, IHostedService =>
        serviceCollection
            .AddSingleton(implementationInstance)
            .AddHostedService<T>(provider => provider.GetRequiredService<T>());
}
