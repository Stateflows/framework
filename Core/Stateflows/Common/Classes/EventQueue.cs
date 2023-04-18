using System;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Common.Utilities;

namespace Stateflows.Common.Classes
{
    internal class EventQueue<T> : LinkedList<T>
    {
        private bool Locked { get; }

        public EventQueue(bool locked)
        {
            Locked = locked;
        }

        private EventWaitHandle Event { get; } = new EventWaitHandle(false, EventResetMode.AutoReset);

        private static readonly string QueueEmpty = "Queue empty.";

        public Task WaitAsync()
            => Event.WaitOneAsync();

        public bool Wait()
            => Event.WaitOne();

        private void DoEnqueue(T item)
        {
            AddLast(item);
            Event.Set();
        }

        public void Enqueue(T item)
        {
            if (Locked)
            {
                lock (this)
                {
                    DoEnqueue(item);
                }
            }
            else
            {
                DoEnqueue(item);
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
                lock (this)
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
                lock (this)
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
                lock (this)
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
