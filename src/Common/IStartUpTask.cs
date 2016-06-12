namespace SimpleScheduler.Common
{
    public interface IStartUpTask
    {
        void Run();
        StartupTaskPriority Priority { get; }
    }
}
