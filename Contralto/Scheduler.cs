using System;
using System.Collections.Generic;


namespace Contralto
{
    /// <summary>
    /// The SchedulerEventCallback describes a delegate that is invoked whenever a scheduled event has
    /// reached its due-date and is fired.
    /// </summary>
    /// <param name="timeNsec">The current Alto time (in nsec)</param>
    /// <param name="skew">The delta between the requested exec time and the actual exec time (in nsec)</param>
    /// <param name="context">An object containing context useful to the scheduler of the event</param>
    public delegate void SchedulerEventCallback(ulong timeNsec, ulong skewNsec, object context);

    /// <summary>
    /// 
    /// </summary>
    public class Event
    {
        public Event(ulong timestampNsec, object context, SchedulerEventCallback callback)
        {
            _timestampNsec = timestampNsec;
            _context = context;
            _callback = callback;
        }

        /// <summary>
        /// The absolute time (in nsec) to raise the event.
        /// </summary>
        public ulong TimestampNsec
        {
            get { return _timestampNsec; }
            set { _timestampNsec = value; }
        }

        /// <summary>
        /// An object containing context to be passed to the
        /// event callback.
        /// </summary>
        public object Context
        {
            get { return _context; }  
            set { _context = value; }          
        }

        /// <summary>
        /// A delegate to be executed when the callback fires.
        /// </summary>
        public SchedulerEventCallback EventCallback
        {
            get { return _callback; }
        }

        private ulong _timestampNsec;
        private object _context;
        private SchedulerEventCallback _callback;
    }

    /// <summary>
    /// The Scheduler class provides infrastructure for scheduling Alto time-based hardware events
    /// (for example, sector marks, or video task wakeups).
    /// </summary>
    public class Scheduler
    {
        public Scheduler()
        {
            Reset();
        }

        public ulong CurrentTimeNsec
        {
            get { return _currentTimeNsec; }
        }

        public void Reset()
        {
            _schedule = new SchedulerQueue();
            _currentTimeNsec = 0;
        }

        public void Clock()
        {
            //
            // Move one system clock forward in time.
            //
            _currentTimeNsec += _timeStepNsec;

            //
            // See if we have any events waiting to fire at this timestep.
            //
            while (true)
            {                
                if (_schedule.Top != null && _currentTimeNsec >= _schedule.Top.TimestampNsec)
                {
                    // Pop the top event and fire the callback.
                    Event e = _schedule.Pop();
                    e.EventCallback(_currentTimeNsec, _currentTimeNsec - e.TimestampNsec, e.Context);
                }
                else
                {
                    // All done.
                    break;
                }
            }
        }


        public Event Schedule(Event e)
        {
            e.TimestampNsec += _currentTimeNsec;
            _schedule.Push(e);

            return e;
        }

        public void CancelEvent(Event e)
        {
            _schedule.Remove(e);
        }

        private ulong _currentTimeNsec;

        private SchedulerQueue _schedule;

        // 170nsec is approximately one Alto system clock cycle and is the time-base for
        // the scheduler.
        private const ulong _timeStepNsec = 170;        
    }

    /// <summary>
    /// Provides an "ordered" queue based on timestamp -- the top of the queue is always the 
    /// next event to be fired; a "push" places a new event in order on the current queue.
    /// </summary>
    public class SchedulerQueue
    {
        public SchedulerQueue()
        {
            _queue = new LinkedList<Event>();
        }

        public Event Top
        {
            get
            {
                if (_queue.Count > 0)
                {
                    return _queue.First.Value;
                }
                else
                {
                    return null;
                }
            }
        }

        public void Push(Event e)
        {
            // Degenerate case:  list is empty or new entry is earlier than the head of the list.
            if (_queue.Count == 0 || _queue.First.Value.TimestampNsec >= e.TimestampNsec)
            {
                _queue.AddFirst(e);
                return;
            }

            //
            // Do a linear search to find the place to put this in.
            // Since we maintain a sorted list with every insertion we only need to find the first entry
            // that the new entry is earlier (or equal) to.
            // This will likely be adequate as the queue should never get incredibly deep; a binary
            // search may be more performant if this is not the case.
            // TODO: profile this and ensure this is the case.
            //
            LinkedListNode<Event> current = _queue.First;
            while (current != null)
            {
                if (current.Value.TimestampNsec >= e.TimestampNsec)
                {
                    _queue.AddBefore(current, e);
                    return;
                }

                current = current.Next;
            }

            // Add at end
            _queue.AddLast(e);
        }

        public Event Pop()
        {           
            Event e = _queue.First.Value;
            _queue.RemoveFirst();

            return e;
        }

        public void Remove(Event e)
        {
            _queue.Remove(e);
        }

        /// <summary>
        /// TODO: provide more optimal data structure here once profiling can be done.
        /// </summary>
        private LinkedList<Event> _queue;
    }
}
