using System;
using System.Threading;

namespace SimpleScheduler.Common
{
    public abstract class ScheduledTask
    {
        private CancellationTokenSource _cancellationToken = new CancellationTokenSource();
        protected CancellationTokenSource CancellationToken
        {
            get { return _cancellationToken; }
        }

        public virtual bool IsRunning { get; set; }

        public virtual bool ShouldSchedule { get { return true; } }

        public virtual void OnScheduleEnded(object sender, EventArgs e)
        {
            CancellationToken.Cancel(); //Cancel any current operations
        }

        protected bool IsCancelRequested { get { return CancellationToken.IsCancellationRequested; } }
    }
}
