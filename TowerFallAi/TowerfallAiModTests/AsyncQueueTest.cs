using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using TowerfallAi.Common;

namespace TowerfallAiTests {
  [TestFixture]
  public class AsyncQueueTest {
    class TestException : Exception { }

    [Test]
    public void MultiEnqueueDequeueTest() {
      // Spawns many threads to enqueue and dequeue from AsyncQueue concurrently.
      var asyncQueue = new AsyncQueue<int>();

      // This is used to block the threads so they are not re
      var enqueueAllEvent = new ManualResetEvent(false);

      int messageCount = 10;
      int workerThreads;
      int completionPortThreads;
      ThreadPool.GetMinThreads(out workerThreads, out completionPortThreads);
      ThreadPool.SetMinThreads(workerThreads + messageCount, completionPortThreads);

      TaskEx.Run(() => {
        for (int i = 0; i < messageCount; i++) {
          TaskEx.Run(() => {
            asyncQueue.Enqueue(Thread.CurrentThread.ManagedThreadId);
            if (i == messageCount - 1) {
              enqueueAllEvent.Set();
            } else {
              enqueueAllEvent.WaitOne();
            }
          });
        }
      });

      List<Task<int>> dequeueTasks = new List<Task<int>>();
      var dequeueTask = TaskEx.Run(() => {
        for (int i = 0; i < messageCount; i++) {
          dequeueTasks.Add(asyncQueue.DequeueAsync());
        }
      });

      dequeueTask.Wait();
      var threadIds = new HashSet<int>();
      foreach (Task<int> task in dequeueTasks) {
        threadIds.Add(task.Result);
      }

      Assert.AreEqual(messageCount, threadIds.Count);
    }

    [Test]
    public void EnqueueExceptionTest() {
      var asyncQueue = new AsyncQueue<int>();
      var dequeueTask = asyncQueue.DequeueAsync();

      asyncQueue.EnqueueException(new TestException());

      AssertException(dequeueTask, typeof(TestException));
    }

    [Test]
    public void CancellationTest() {
      var asyncQueue = new AsyncQueue<int>();
      CancellationTokenSource cancelTs = new CancellationTokenSource();

      var dequeueTask = asyncQueue.DequeueAsync(cancellationToken: cancelTs.Token);

      AssertAlive(dequeueTask);

      Task<int> dependentTask = TaskEx.Run(async () => {
        return await dequeueTask + 1;
      });

      AssertAlive(dependentTask);

      cancelTs.Cancel();

      AssertException(dependentTask, typeof(TaskCanceledException));
      AssertException(dequeueTask, typeof(TaskCanceledException));

      Assert.AreEqual(TaskStatus.Canceled, dependentTask.Status);
      Assert.AreEqual(TaskStatus.Canceled, dequeueTask.Status);
    }

    [Test]
    public void EnqueueFirstTest() {
      var asyncQueue = new AsyncQueue<int>();

      for (int i = 0; i < 10; i++) {
        asyncQueue.Enqueue(i);
      }

      for (int i = 0; i < 10; i++) {
        Assert.AreEqual(i, asyncQueue.DequeueAsync().Result);
      }
    }

    [Test]
    public void EnqueueLastCheckOrderTest() {
      var asyncQueue = new AsyncQueue<int>();

      var dequeueTasks = new List<Task<int>>();
      for (int i = 0; i < 10; i++) {
        dequeueTasks.Add(asyncQueue.DequeueAsync());
      }

      for (int i = 0; i < 10; i++) {
        asyncQueue.Enqueue(i);
      }

      for (int i = 0; i < 10; i++) {
        Assert.AreEqual(i, dequeueTasks[i].Result);
      }
    }

    [Test]
    public void CloseTest() {
      var asyncQueue = new AsyncQueue<int>();
      asyncQueue.Enqueue(42);
      asyncQueue.Close();

      Assert.AreEqual(42, asyncQueue.DequeueAsync().Result);

      Assert.Throws<InvalidOperationException>(() => asyncQueue.Enqueue(10));
      Assert.Throws<InvalidOperationException>(() => asyncQueue.DequeueAsync());
      Assert.Throws<InvalidOperationException>(() => asyncQueue.EnqueueException(new TestException()));
    }

    [Test]
    public void TimeoutTest() {
      var asyncQueue = new AsyncQueue<int>();

      int delayTimeMs = 50;
      var dequeueTask = asyncQueue.DequeueAsync(new TimeSpan(0, 0, 0, 0, delayTimeMs));
      asyncQueue.Enqueue(42);
      int result = dequeueTask.Result;
      Assert.AreEqual(42, result);

      // This is necessary to make sure nothing weird happens after timeout period.
      Thread.Sleep(delayTimeMs);

      delayTimeMs = 50;
      dequeueTask = asyncQueue.DequeueAsync(new TimeSpan(0, 0, 0, 0, delayTimeMs));
      var dependentTask = TaskEx.Run(async () => {
        return await dequeueTask;
      });

      AssertAlive(dequeueTask);

      TaskEx.WhenAny(dependentTask, TaskEx.Delay(2*delayTimeMs)).Wait();

      Assert.AreEqual(TaskStatus.Faulted, dequeueTask.Status);
      Assert.AreEqual(TaskStatus.Faulted, dependentTask.Status);

      AssertException(dequeueTask, typeof(TimeoutException));
      AssertException(dequeueTask, typeof(TimeoutException));
    }

    private void AssertAlive(Task task) {
      Assert.AreNotEqual(TaskStatus.RanToCompletion, task.Status);
      Assert.AreNotEqual(TaskStatus.Faulted, task.Status);
      Assert.AreNotEqual(TaskStatus.Canceled, task.Status);
    }

    private void AssertException(Task task, Type expectedExceptionType) {
      try {
        task.Wait();
        Assert.Fail("Exception was not thrown.");
      } catch (AggregateException aggregateException) {
        Assert.AreEqual(1, aggregateException.InnerExceptions.Count);
        Assert.AreEqual(expectedExceptionType, aggregateException.InnerException.GetType());
      }
    }
  }
}
