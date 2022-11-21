namespace Coravel.WorkerService
{
    using System;
    using Coravel;
    using Coravel.Scheduling.Schedule.Interfaces;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    public static class WorkerWebHost
    {
        public static IHost EnableWorkerService(this IHost host)
        {
            var schedules = host.Services.GetServices<Interfaces.ICustomScheduler>();
            var onErrorHandlers = host.Services.GetServices<Interfaces.ICustomExcepcionHandler>();
            var logger = (ILogger<IScheduler>)host.Services.GetService(typeof(ILogger<IScheduler>));

            host.Services
            .UseScheduler(scheduler =>
            {
                foreach (var schedule in schedules)
                {
                    schedule.AddSchedule(scheduler);
                }
            })
            .LogScheduledTaskProgress(logger)
            .OnError((exception) =>
            {

                if (onErrorHandlers == null)
                    return;
                foreach (var errorHandler in onErrorHandlers)
                {
                    try
                    {
                        errorHandler.CatchException(exception);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Custom Exception Handler Failed.");
                    }
                }
            });

            return host;
        }
    }
}
