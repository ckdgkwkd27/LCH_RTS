namespace LCH_RTS.Job;

internal struct JobTimerElem : IComparable<JobTimerElem>
{
    public int execTick;
    public IJob job;

    public int CompareTo(JobTimerElem other)
    {
        return other.execTick - execTick;
    }
}

public class JobTimer
{
    private readonly PriorityQueue<JobTimerElem, int> _pq = new();
    private readonly Lock _lock = new();

    public void Push(IJob job, int tickAfter = 0)
    {
        JobTimerElem elem;
        elem.execTick = Environment.TickCount + tickAfter;
        elem.job = job;

        using (_lock.EnterScope())
        {
            _pq.Enqueue(elem, elem.execTick);
        }
    }
    
    public void Flush()
    {
        while (true)
        {
            var now = System.Environment.TickCount;

            JobTimerElem jobElement;

            using (_lock.EnterScope())
            {
                if (_pq.Count == 0)
                    break;

                jobElement = _pq.Peek();
                if (jobElement.execTick > now)
                    break;

                _pq.Dequeue();
            }

            jobElement.job.Execute();
        }
    }
}