using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Common {
    public class KeySemaphore {
        object locker = new object();
        Dictionary<string, Semaphore> semaphores = new Dictionary<string, Semaphore>();

        public void Run(string key, Action action) {
            Semaphore semaphore = null;

            Exception exception = null;

            try {
                lock (locker) {
                    if (!semaphores.TryGetValue(key, out semaphore)) {
                        semaphore = new Semaphore(1, 1);
                        semaphores[key] = semaphore;
                    }
                }

                semaphore.WaitOne();
                action();
            } catch (Exception ex) {
                exception = ex;
            }

            lock (locker) {
                if (semaphore != null) {
                    int c = semaphore.Release(1);
                    if (c == 0 && semaphores.ContainsKey(key)) {
                        semaphores.Remove(key);
                    }
                }
            }

            if (exception != null) {
                throw exception;
            }
        }
    }
}
