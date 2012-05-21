using System;
using System.Collections.Generic;

namespace Treefrog.Framework
{
    public class ServiceContainer : IServiceProvider
    {
        private static readonly ServiceContainer _default = new ServiceContainer();

        private readonly object _lock;
        private readonly Dictionary<Type, object> _registry;

        public ServiceContainer ()
        {
            _lock = new object();
            _registry = new Dictionary<Type, object>();
        }

        public static ServiceContainer Default
        {
            get { return _default; }
        }

        public void AddService (Type serviceType, object service)
        {
            lock (_lock) {
                _registry[serviceType] = service;
            }
        }

        public void AddService<TService> (TService service)
        {
            lock (_lock) {
                _registry[typeof(TService)] = service;
            }
        }

        public object GetService (Type serviceType)
        {
            object service;
            lock (_lock) {
                _registry.TryGetValue(serviceType, out service);
            }
            return service;
        }

        public TService GetService<TService> ()
            where TService : class
        {
            object service;
            lock (_lock) {
                _registry.TryGetValue(typeof(TService), out service);
            }
            return service as TService;
        }
    }

    public static class IServiceProviderExtensions
    {
        public static TService GetService<TService> (this IServiceProvider container)
            where TService : class
        {
            return container.GetService(typeof(TService)) as TService;
        }
    }
}
