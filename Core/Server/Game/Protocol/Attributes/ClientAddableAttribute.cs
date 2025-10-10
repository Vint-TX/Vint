using JetBrains.Annotations;

namespace Vint.Core.Server.Game.Protocol.Attributes;

[AttributeUsage(AttributeTargets.Class), MeansImplicitUse(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public class ClientAddableAttribute : Attribute;
