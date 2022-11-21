namespace Coravel.WorkerService.Abstractions
{
    using Coravel.Scheduling.Schedule.Interfaces;
    using Coravel.WorkerService.Interfaces;
    using Microsoft.Extensions.Configuration;

    public abstract class BaseConfigurableSchedule : ICustomScheduler
    {
        protected readonly IConfiguration configuration;
        protected BaseConfigurableSchedule(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public abstract void AddSchedule(IScheduler scheduler);
    }
}
