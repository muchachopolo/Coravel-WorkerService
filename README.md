# Worker Service for .Net Core Apps

## Objective

This Class Library was meant to let the developer apply the Coravel configuration using DI and automatic class detection.
Scan and register all the schedules and classes using dark magic.

## How to Use

- Install using NuGet Coravel.WorkerService Package
- In the program.cs stratup class add the EnableWorkerService method after the Host Build
```csharp
    using Coravel.WorkerService;

    CreateWebHostBuilder(args).Build().EnableWorkerService().Run();
```
- Add ConfigureService Method in the Startup Services addning the assembly names where the jobs and schedules will be implemented

```csharp
    using Microsoft.Extensions.DependencyInjection;

    services.ConfigureScheduler(new[]
    {
        "MyApp.API",
        "MyApp.WorkerService"
    });
```


### Jobs and Schedules

* Jobs and Schedules will automatically added to the service collection container and registered at startup secuence
 
#### Jobs

In order to add Jobs just create a Class implementing the IInvocable Interface from the Coravel.Invocable namespace included in Coravel.WorkerService Package

##### Example
```csharp
    using Coravel.Invocable;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Threading.Tasks;
    public class WorkerHealthCheckJob : IInvocable
    {
        private readonly ILogger<WorkerHealthCheckJob> logger;

        public WorkerHealthCheckJob(ILogger<WorkerHealthCheckJob> logger)
        {
            this.logger = logger;
        }
        public async Task Invoke()
        {
            var message = String.Format("This message means the Worker Service is working. Date: {0}", DateTime.Now);
            logger.LogInformation(message);
            await Task.CompletedTask;
        }
    }
```
#### Schedules

In order to add Schedules just create a Class implementing BaseConfigurableSchedule abstract class or ICustomScheduler Interface from the Coravel.WorkerService namespace

##### Example
```csharp
    using System;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Coravel.Scheduling.Schedule.Interfaces;
    using Cuotas.WorkerService.Jobs;
    using Coravel.WorkerService.Abstractions;

    public class WorkerHealthCheckSchedule : BaseConfigurableSchedule
    {
        private readonly ILogger<WorkerHealthCheckSchedule> logger;
        private const string WORKER_HEALTH_CHECK_SCHEDULE_TIME = "Scheduler:WorkerHealthCheckScheduleTime";

        public WorkerHealthCheckSchedule(IConfiguration configuration, ILogger<WorkerHealthCheckSchedule> logger) : base(configuration)
        {
            this.logger = logger;
        }

        public override void AddSchedule(IScheduler scheduler)
        {
            string scheduleTime = configuration.GetSection(WORKER_HEALTH_CHECK_SCHEDULE_TIME).Value ?? "0 * * * *";
            logger.LogInformation(String.Format("Health Check Schedule set the following Cron time: {0}", scheduleTime));

            scheduler.OnWorker("General");
            scheduler
                .Schedule<WorkerHealthCheckJob>()
                .Cron(scheduleTime);
        }
    }
```

#### Exception Handlers

In order to add ExceptionHandlers just create a Class implementing ICustomExceptionHandler Interface from the Coravel.WorkerService namespace
* the exception will be logged even if you add a custom exception hanlder by Coravel implementation.

# Worker Service - More Information

The worker service has the Schedule jobs configured in the class `WorkerWebHost.cs`
Example:
```csharp
host.Services.UseScheduler(scheduler =>
{
    // Job 1
    scheduler
        .Schedule<SampleJob>()
        .EverySeconds(10);
    // Job 2    
    scheduler
        .Schedule<HealthCheckJob>()
        .DailyAtHour(0)
        .Weekday();
});
```

Example2
```csharp
scheduler.OnWorker("NaughtyWorker");
scheduler
    .Schedule<PutNaughtyChildrenFromAPIIntoDb>()
    .Hourly();

scheduler.OnWorker("EmailTasks");
scheduler
    .Schedule<SendNightlyReportsEmailJob>().Daily();
scheduler
    .Schedule<SendPendingNotifications>().EveryMinute();

scheduler.OnWorker("CPUIntensiveTasks");
scheduler
    .Schedule<RebuildStaticCachedData>()
    .Hourly()
    .PreventOverlapping("CPUIntensiveTasks");
```
### IoC Container

Every job class have to be registered in the `IoC container` in the class `IoC\WorkerServiceCollecion.cs`

Example:

```csharp
public static IServiceCollection ConfigureScheduler(this IServiceCollection services)
{
    services.AddScheduler();
    services.AddTransient(typeof(JobHelper<>));
    //Job 1 Registered as Transient
    services.AddTransient<SampleJob>();
    //Job 2 Registered as Singleton
    services.AddSingleton<HealthCheckJob>();

    return services;
}
```

### Exeption Handler

Currently the Worker Service uses the default exception hlandler that will show a message using the ILogger.
The message in the logs will say: `Coravel.Scheduling.Schedule.Interfaces.IScheduler:Error: A scheduled task threw an Exception:`

##### Customization 

We can add the following code snipet to change the behaviour 

```csharp
provider.UseScheduler(scheduler =>
    // Assign your schedules
)
.OnError((exception) =>
    DoSomethingWithException(exception)
);
```


### Dependencies

``Coravel`` is being used as a nuget dependency in order to implement the worker service.

#### Features

Coravel has the following features:

##### Task Scheduling
Usually, you have to configure a cron job or a task via Windows Task Scheduler to get a single or multiple re-occurring tasks to run.

With Coravel, you can setup all your scheduled tasks in one place using a simple, elegant, fluent syntax - in code!

##### Queuing
Coravel gives you a zero-configuration queue that runs in-memory to offload long-winded tasks to the background instead of making your users wait for their HTTP request to finish!

##### Caching
Coravel provides you with an easy to use API for caching in your .NET Core applications.

By default, it uses an in-memory cache, but also has database drivers for more robust scenarios!

##### Event Broadcasting
Coravel's event broadcasting helps you to build maintainable applications who's parts are loosely coupled!

##### Mailing
E-mails are not as easy as they should be. Luckily for you, Coravel solves this by offering:

Built-in e-mail friendly razor templates
Simple and flexible mailing API
Render your e-mails for visual testing
Drivers supporting SMTP, local log file or BYOM ("bring your own mailer") driver
Quick and simple configuration via appsettings.json

https://github.com/jamesmh/coravel
