namespace Vint.Core.Battles;

public readonly record struct DelayedAction(
    DateTimeOffset InvokeAtTime,
    Action Action
);