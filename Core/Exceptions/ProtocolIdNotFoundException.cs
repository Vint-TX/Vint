namespace Vint.Core.Exceptions;

public class ProtocolIdNotFoundException(Type type) : Exception($"Protocol Id attribute not found on {type.FullName}");
