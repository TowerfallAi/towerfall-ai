using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TowerfallAi.Common {
  public class AsyncQueue<T> {
    private Queue<TaskCompletionSource<T>> completedTasks = new Queue<TaskCompletionSource<T>>();
    private Queue<TaskCompletionSource<T>> waitingTasks = new Queue<TaskCompletionSource<T>>();
    private object queueLock = new object();
    public bool IsClosed { get; private set; }

    public Task<T> DequeueAsync(TimeSpan timeout = default, CancellationToken cancellationToken = default) {
      lock (queueLock) {
        if (completedTasks.Count > 0) {
          return completedTasks.Dequeue().Task;
        }

        if (IsClosed) {
          throw new InvalidOperationException("Can't dequeue from a closed AsyncQueue.");
        }

        var tcs = new TaskCompletionSource<T>();
        if (cancellationToken != default) {
          cancellationToken.Register(() => HandleManualCancellation(tcs));
        }
        
        if (timeout != default) {
          var timoutTask = FailAfterTimeout(timeout, tcs);
        }
        
        waitingTasks.Enqueue(tcs);
        return tcs.Task;
      }
    }

    public void EnqueueException(Exception ex) {
      TaskCompletionSource<T> tcs;
      lock (queueLock) {
        if (IsClosed) {
          throw new InvalidOperationException("Can't dequeue from a closed AsyncQueue.");
        }

        tcs = DequeueTillNextRunning();

        if (tcs == null) {
          tcs = new TaskCompletionSource<T>();
          completedTasks.Enqueue(tcs);
        }
      }
      tcs.SetException(ex);
    }

    public void Enqueue(T result) {
      TaskCompletionSource<T> tcs = null;
      lock (queueLock) {
        if (IsClosed) {
          throw new InvalidOperationException("Can't dequeue from a closed AsyncQueue.");
        }

        tcs = DequeueTillNextRunning();
        
        if (tcs == null) {
          tcs = new TaskCompletionSource<T>();
          completedTasks.Enqueue(tcs);
        }
      }
      tcs.SetResult(result);
    }

    public void Close() {
      lock (queueLock) {
        IsClosed = true;
      }
    }

    private TaskCompletionSource<T> DequeueTillNextRunning() {
      // Look for existing requests for results in waitingTasks. Some might have been cancelled or timed out.
      // So select the ones that are still running.
      if (waitingTasks.Count > 0) {
        TaskCompletionSource<T> tcs = waitingTasks.Dequeue();
        if (tcs.Task.IsAlive()) {
          return tcs;
        }
      }

      return null;
    }

    private void HandleManualCancellation(TaskCompletionSource<T> tcs) {
      lock (queueLock) {
        if (tcs.Task.IsAlive()) {
          tcs.SetCanceled();
        }
      }
    }

    private async Task FailAfterTimeout(TimeSpan timeout, TaskCompletionSource<T> tcs) {
      await TaskEx.Delay(timeout);
      Console.WriteLine($"Timeout: {timeout} {tcs.Task.Status}");
      lock (queueLock) {
        if (tcs.Task.IsAlive()) {
          tcs.SetException(new TimeoutException($"Timeout: {timeout}"));
          Console.WriteLine("Task was set to time out");
        }
      }
    }
  }
}
