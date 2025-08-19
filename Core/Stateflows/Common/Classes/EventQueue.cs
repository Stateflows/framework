using System;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Stateflows.Common.Utilities;
using System.Runtime.Serialization;

namespace Stateflows.Common.Classes
{
    [Serializable]
    public class EventQueue<T> : LinkedList<T>
    {
        private readonly object LockObject = new object();

        private readonly bool Locked;

        public EventQueue(bool locked)
        {
            Locked = locked;
        }

        protected EventQueue(SerializationInfo info, StreamingContext context) : base(info, context)
        { }

        private readonly EventWaitHandle Event = new EventWaitHandle(false, EventResetMode.AutoReset);

        private readonly string QueueEmpty = "Queue empty.";

        [DebuggerHidden]
        public async Task<bool> WaitAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await WaitAsync(10 * 1000);

                var enqueued = false;

                if (Locked)
                {
                    lock (LockObject)
                    {
                        enqueued = Count > 0;
                    }
                }
                else
                {
                    enqueued = Count > 0;
                }

                if (enqueued)
                {
                    return true;
                }
            }

            return false;
        }

        public Task WaitAsync(int millisecondsTimeout = -1)
            => Event.WaitOneAsync(millisecondsTimeout);

        public bool Wait()
            => Event.WaitOne();

        private void DoEnqueue(T item)
        {
            AddLast(item);
            Event.Set();
        }

        public int Enqueue(T item)
        {
            if (Locked)
            {
                lock (LockObject)
                {
                    DoEnqueue(item);
                    return Count;
                }
            }
            else
            {
                DoEnqueue(item);
                return Count;
            }
        }

        private void DoPush(T item)
        {
            AddFirst(item);
            Event.Set();
        }

        public void Push(T item)
        {
            if (Locked)
            {
                lock (LockObject)
                {
                    DoPush(item);
                }
            }
            else
            {
                DoPush(item);
            }
        }

        private T DoPeek()
        {
            if (Count == 0)
            {
                throw new InvalidOperationException(QueueEmpty);
            }

            return First.Value;
        }

        public T Peek()
        {
            if (Locked)
            {
                lock (LockObject)
                {
                    return DoPeek();
                }
            }
            else
            {
                return DoPeek();
            }
        }

        private T DoDequeue()
        {
            if (Count == 0)
            {
                throw new InvalidOperationException(QueueEmpty);
            }

            var item = First.Value;
            RemoveFirst();
            if (Count > 0)
            {
                Event.Set();
            }

            return item;
        }

        public T Dequeue()
        {

            if (Locked)
            {
                lock (LockObject)
                {
                    return DoDequeue();
                }
            }
            else
            {
                return DoDequeue();
            }
        }
    }
}
