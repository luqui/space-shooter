using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaceBattle
{
    class Scheduler
    {
        float curtime = 0;
        struct Schedule
        {
            public Schedule(float firetime_, Action action_) { firetime = firetime_; action = action_; }
            public float firetime;
            public Action action;
        };
        List<Schedule> sched = new List<Schedule>();

        public void Update(float dt)
        {
            float target = curtime + dt;
            while (sched.Count > 0)
            {
                if (sched[0].firetime < target)
                {
                    Action action = sched[0].action;
                    curtime = sched[0].firetime;
                    sched.RemoveAt(0);
                    action();
                }
                else
                {
                    break;
                }
            }
            curtime = target;
        }

        public void Enqueue(float dt, Action action) 
        {
            sched.Add(new Schedule(curtime + dt, action));
            sched.Sort(new Comparison<Schedule>((s,t) => s.firetime.CompareTo(t.firetime)));
        }
    }
}
