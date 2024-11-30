namespace Vint.Core.Config;

public readonly record struct DelayedTask(
    DateTimeOffset InvokeAtTime,
    Func<Task> Task
);
