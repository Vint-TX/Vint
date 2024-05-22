namespace Vint.Core.Config;

public readonly record struct DelayedAction(
    DateTimeOffset InvokeAtTime,
    Action Action
);

public readonly record struct DelayedTask(
    DateTimeOffset InvokeAtTime,
    Func<Task> Task
);
