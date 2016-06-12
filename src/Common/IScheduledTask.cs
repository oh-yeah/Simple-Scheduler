using System;

namespace SimpleScheduler.Common
{
    public interface IScheduledTask
    {
        bool ShouldSchedule { get; }
        bool IsRunning { get; set; }
        bool RunOnStartup { get; }
        TimeSpan Interval { get; }

        void Run();
        void OnScheduleEnded(object sender, EventArgs e);
    }
}
