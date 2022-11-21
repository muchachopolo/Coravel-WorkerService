namespace Coravel.WorkerService.Interfaces
{
    using Coravel.Scheduling.Schedule.Interfaces;

    public interface ICustomScheduler
    {
        void AddSchedule(IScheduler scheduler);
    }
}
