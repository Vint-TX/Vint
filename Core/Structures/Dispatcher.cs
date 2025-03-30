using System.Collections.Concurrent;

namespace Vint.Core.Structures;

/// <summary>
/// Executes tasks on a single dedicated thread, allowing callers to block or await for completion.
/// </summary>
public class Dispatcher : IDisposable {
    readonly BlockingCollection<WorkItem> _workItems = new();
    readonly Thread _dispatcherThread;
    bool _isDisposed;

    /// <summary>
    /// Initializes a new instance of the Dispatcher class.
    /// </summary>
    public Dispatcher() {
        _dispatcherThread = new Thread(ProcessWorkItems) {
            Name = "DispatcherThread",
            IsBackground = true
        };
        _dispatcherThread.Start();
    }

    /// <summary>
    /// Gets a value indicating whether the current thread is the dispatcher thread.
    /// </summary>
    public bool CheckAccess() => Environment.CurrentManagedThreadId == _dispatcherThread.ManagedThreadId;

    /// <summary>
    /// Throws an exception if the current thread is not the dispatcher thread.
    /// </summary>
    public void VerifyAccess() {
        if (!CheckAccess())
            throw new InvalidOperationException("This operation must be performed on the dispatcher thread.");
    }

    /// <summary>
    /// Executes an action on the dispatcher thread and blocks until it completes.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    public void Invoke(Action action) {
        if (CheckAccess()) {
            action();
            return;
        }

        ManualResetEventSlim waitHandle = new(false);
        Exception? exception = null;

        _workItems.Add(new WorkItem(() => {
            try {
                action();
            } catch (Exception ex) {
                exception = ex;
            } finally {
                waitHandle.Set();
            }
        }));

        waitHandle.Wait();

        if (exception != null)
            throw new AggregateException("Exception occurred on dispatcher thread", exception);
    }

    /// <summary>
    /// Executes a function on the dispatcher thread and blocks until it completes.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="func">The function to execute.</param>
    /// <returns>The result of the function.</returns>
    public TResult Invoke<TResult>(Func<TResult> func) {
        if (CheckAccess())
            return func();

        ManualResetEventSlim waitHandle = new(false);
        Exception? exception = null;
        TResult result = default!;

        _workItems.Add(new WorkItem(() => {
            try {
                result = func();
            } catch (Exception ex) {
                exception = ex;
            } finally {
                waitHandle.Set();
            }
        }));

        waitHandle.Wait();

        if (exception != null)
            throw new AggregateException("Exception occurred on dispatcher thread", exception);

        return result;
    }

    /// <summary>
    /// Executes an action on the dispatcher thread asynchronously.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <returns>A task that completes when the action completes.</returns>
    public Task InvokeAsync(Action action) {
        if (CheckAccess()) {
            try {
                action();
                return Task.CompletedTask;
            } catch (Exception ex) {
                return Task.FromException(ex);
            }
        }

        TaskCompletionSource tcs = new();

        _workItems.Add(new WorkItem(() => {
            try {
                action();
                tcs.SetResult();
            } catch (Exception ex) {
                tcs.SetException(ex);
            }
        }));

        return tcs.Task;
    }

    /// <summary>
    /// Executes a function on the dispatcher thread asynchronously.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="func">The function to execute.</param>
    /// <returns>A task that completes with the result of the function.</returns>
    public Task<TResult> InvokeAsync<TResult>(Func<TResult> func) {
        if (CheckAccess()) {
            try {
                return Task.FromResult(func());
            } catch (Exception ex) {
                return Task.FromException<TResult>(ex);
            }
        }

        TaskCompletionSource<TResult> tcs = new();

        _workItems.Add(new WorkItem(() => {
            try {
                tcs.SetResult(func());
            } catch (Exception ex) {
                tcs.SetException(ex);
            }
        }));

        return tcs.Task;
    }

    /// <summary>
    /// Executes a task-returning function on the dispatcher thread asynchronously.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="func">The function that returns a task.</param>
    /// <returns>A task that completes with the result of the function.</returns>
    public async Task<TResult> InvokeAsync<TResult>(Func<Task<TResult>> func) {
        if (CheckAccess())
            return await func();

        TaskCompletionSource<TResult> tcs = new();

        _workItems.Add(new WorkItem(async () => {
            try {
                TResult result = await func();
                tcs.SetResult(result);
            } catch (Exception ex) {
                tcs.SetException(ex);
            }
        }));

        return await tcs.Task;
    }

    /// <summary>
    /// Executes a task-returning function on the dispatcher thread asynchronously.
    /// </summary>
    /// <param name="func">The function that returns a task.</param>
    /// <returns>A task that completes when the function's task completes.</returns>
    public async Task InvokeAsync(Func<Task> func) {
        if (CheckAccess()) {
            await func();
            return;
        }

        TaskCompletionSource tcs = new();

        _workItems.Add(new WorkItem(async () => {
            try {
                await func();
                tcs.SetResult();
            } catch (Exception ex) {
                tcs.SetException(ex);
            }
        }));

        await tcs.Task;
    }

    /// <summary>
    /// Disposes the dispatcher, stopping the processing of work items.
    /// </summary>
    public void Dispose() {
        if (_isDisposed)
            return;

        _isDisposed = true;
        _workItems.CompleteAdding();

        // Give the thread a chance to finish processing
        if (!Thread.CurrentThread.Equals(_dispatcherThread))
            _dispatcherThread.Join(1000);

        _workItems.Dispose();
        GC.SuppressFinalize(this);
    }

    void ProcessWorkItems() {
        try {
            foreach (WorkItem workItem in _workItems.GetConsumingEnumerable()) {
                if (_isDisposed)
                    break;

                workItem.Execute();
            }
        } catch (ObjectDisposedException) {
            // Expected when disposed
        } catch (InvalidOperationException) {
            // Expected when collection is completed
        }
    }

    /// <summary>
    /// Represents a work item to be executed on the dispatcher thread.
    /// </summary>
    class WorkItem {
        readonly Delegate _action;

        public WorkItem(Action action) => _action = action;

        public WorkItem(Func<Task> asyncAction) => _action = asyncAction;

        public void Execute() {
            switch (_action) {
                case Action action:
                    action();
                    break;

                case Func<Task> asyncAction:
                    // Fire and forget, but with error handling
                    asyncAction().ContinueWith(t => {
                        if (t.IsFaulted)
                            Console.Error.WriteLine($"Unhandled exception in async dispatcher task: {t.Exception}");
                    }, TaskContinuationOptions.OnlyOnFaulted);
                    break;
            }
        }
    }
}
