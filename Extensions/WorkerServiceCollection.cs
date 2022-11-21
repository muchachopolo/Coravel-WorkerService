namespace Microsoft.Extensions.DependencyInjection
{
    using Coravel;
    using Coravel.Invocable;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Coravel.WorkerService.Interfaces;

    public static class WorkerServiceCollection
    {
        public static IServiceCollection ConfigureScheduler(this IServiceCollection services, params string[] assemblyNamesToScan) 
            => ConfigureScheduler(services, ServiceLifetime.Transient, assemblyNamesToScan);

        public static IServiceCollection ConfigureScheduler(
            this IServiceCollection services,
            ServiceLifetime lifetime,
            params string[] assemblyNamesToScan)
        {
            services.AddScheduler();
            var allAsemblies = assemblyNamesToScan.Select(Assembly.Load);
            
            var allSchedulerTypes = allAsemblies
                .Where(a => !a.IsDynamic).SelectMany(a => a.DefinedTypes)
                .Where(t => t.IsClass && typeof(ICustomScheduler).IsAssignableFrom(t) && !t.IsAbstract)
                .ToList();

            var allJobTypes = allAsemblies
                .Where(a => !a.IsDynamic).SelectMany(a => a.DefinedTypes)
                .Where(t => t.IsClass && typeof(IInvocable).IsAssignableFrom(t) && !t.IsAbstract)
                .ToArray();

            var allExceptionHanlderTypes = allAsemblies
                .Where(a => !a.IsDynamic).SelectMany(a => a.DefinedTypes)
                .Where(t => t.IsClass && typeof(ICustomExcepcionHandler).IsAssignableFrom(t) && !t.IsAbstract)
                .ToArray();

            AddAllServices(services, allSchedulerTypes, typeof(ICustomScheduler), lifetime);
            AddAllServices(services, allJobTypes, lifetime);
            AddAllServices(services, allExceptionHanlderTypes, typeof(ICustomExcepcionHandler), lifetime);

            return services;
        }

        private static void AddAllServices(IServiceCollection services, IEnumerable<TypeInfo> typeImplementationInfoList, ServiceLifetime lifetime)
        {
            foreach (var typeInfo in typeImplementationInfoList)
            {
                switch(lifetime)
                {
                    case ServiceLifetime.Transient:
                        services.AddTransient(typeInfo.AsType());
                        break;
                    case ServiceLifetime.Scoped:
                        services.AddScoped(typeInfo.AsType());
                        break;
                    case ServiceLifetime.Singleton:
                        services.AddSingleton(typeInfo.AsType());
                        break;
                }
            }
        }

        private static void AddAllServices(IServiceCollection services, IEnumerable<TypeInfo> typeImplementationInfoList, Type typeService, ServiceLifetime lifetime)
        {
            foreach (var type in typeImplementationInfoList)
            {
                services.Add(new ServiceDescriptor(typeService, type.AsType(), lifetime));
            }
        }
    }
}
