using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SimpleScheduler.Common;

namespace SimpleScheduler
{
    internal abstract class TaskScheduler
    {
        #region Storage

        private readonly IEnumerable<IScheduledTask> _scheduledTasks;
        private readonly IList<Timer> _timers = new List<Timer>();
        private readonly IList<IStartUpTask> _startupTasks;

        #endregion

        #region Constructors

        //Empty
        protected TaskScheduler() {}

        //Only startup tasks
        protected TaskScheduler(IEnumerable<IStartUpTask> startupTasks)
        {
            _startupTasks = startupTasks.ToList();
        }

        //Only scheduled tasks
        protected TaskScheduler(IEnumerable<IScheduledTask> scheduledTasks)
        {
            _scheduledTasks = scheduledTasks;
        }

        //Both startup and scheduled tasks
        protected TaskScheduler(IEnumerable<IStartUpTask> startupTasks, IEnumerable<IScheduledTask> scheduledTasks)
            : this(startupTasks)
        {
            _scheduledTasks = scheduledTasks;
        }

        #endregion

        #region Behavior

        public virtual void Start()
        {
            RunStartupTasks();
            RegisterTasks();
        }

        public virtual void Stop()
        {
            //Raise the event
            RaiseScheduleStopped();

            //Remove the event handlers
            _scheduledTasks.ToList().ForEach(x => ScheduleStopped -= x.OnScheduleEnded); 

            //Clear the timers
            _timers.ToList().ForEach(x => x.Dispose());
            _timers.Clear();
        }

        //Run all startup tasks
        private void RunStartupTasks()
        {
            if (_startupTasks != null && _startupTasks.Any())
            {
                _startupTasks.OrderBy(x => (int)x.Priority)
                             .ToList().ForEach(x =>
                             {
                                 x.Run();
                             });

                _startupTasks.Clear(); //Remove all the tasks, no need to hold onto them
            }
        }

        //Registers all scheduled tasks in the container
        private void RegisterTasks()
        {
            if (_scheduledTasks != null && _scheduledTasks.Any())
            {
                foreach (var task in _scheduledTasks.Where(x => x.ShouldSchedule)) //Only schedule tasks that should be scheduled
                {
                    //Register to the stop event
                    ScheduleStopped += task.OnScheduleEnded;

                    //Add a timer for the task
                    _timers.Add(new Timer(_ => RunTask(task), null, TimeSpan.Zero, task.Interval));

                    if (task.RunOnStartup) //Run the task if necessary
                    {
                        RunTask(task);
                    }
                }
            }
        }

        //Executes a scheduled task asynchronously
        private void RunTask(IScheduledTask task)
        {
            if (!task.IsRunning)
            {
                Task.Run(() =>
                {
                    task.Run();
                });
            }
        }

        #endregion

        #region Events

        public event EventHandler ScheduleStopped;
        private void RaiseScheduleStopped()
        {
            ScheduleStopped?.Invoke(this, new EventArgs());
        }

        #endregion
    }
}
