namespace Vint.Core.Exceptions;

public class TypeNotRegisteredException(long id) : Exception($"Type with id '{id}' is not registered");
