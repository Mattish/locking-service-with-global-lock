using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TaskLockingResearch
{
    public class LockingItem : IDisposable
    {
        private readonly object _lockObj;
        private readonly Action _disposeAction;

        public LockingItem(object lockObj, Action disposeAction)
        {
            _lockObj = lockObj;
            _disposeAction = disposeAction;
        }

        public void Dispose()
        {
            Monitor.Exit(_lockObj);
            _disposeAction();
        }
    }

    public class LockingService
    {
        private Dictionary<Guid, object> _lockingDictionary = new Dictionary<Guid, object>();
        private Dictionary<int, int> _taskDictionary = new Dictionary<int, int>();

        private object _globalLock = new object();

        public IDisposable GetLock(Guid id)
        {
            int taskLockCount;
            bool isInDictionary;
            lock (_taskDictionary)
                isInDictionary = _taskDictionary.TryGetValue(Task.CurrentId.Value, out taskLockCount);

            if (!isInDictionary)
            {
                lock (_globalLock)
                    _taskDictionary[Task.CurrentId.Value] = 1;
            }
            else
            {
                lock (_taskDictionary)
                    _taskDictionary[Task.CurrentId.Value]++;
            }

            object lockObj;
            lock (_lockingDictionary)
            {
                if (!_lockingDictionary.TryGetValue(id, out lockObj))
                {
                    lockObj = new object();
                    _lockingDictionary[id] = lockObj;
                }
            }

            Monitor.Enter(lockObj);
            return new LockingItem(lockObj, () =>
            {
                lock (_taskDictionary)
                {
                    _taskDictionary[Task.CurrentId.Value]--;
                    if (_taskDictionary[Task.CurrentId.Value] == 0)
                    {
                        _taskDictionary.Remove(Task.CurrentId.Value);
                    }
                }
            });
        }

        public IDisposable GetGlobalLock()
        {
            Monitor.Enter(_globalLock);
            SpinWait.SpinUntil(() => _taskDictionary.Count == 0);
            Monitor.Enter(_lockingDictionary);
            return new LockingItem(_globalLock, () => { Monitor.Exit(_lockingDictionary); });
        }
    }
}