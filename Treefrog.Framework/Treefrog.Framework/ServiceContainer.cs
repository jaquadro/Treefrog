using System;
using System.Collections.Generic;

namespace Treefrog.Framework
{
    public class ServiceContainer : IServiceProvider
    {
        private Dictionary<Type, object> _services = new Dictionary<Type, object>();

        public void AddService<T> (T service)
        {
            _services.Add(typeof(T), service);
        }

        public void AddService (Type serviceType, object provider)
        {
            _services.Add(serviceType, provider);
        }

        public object GetService (Type serviceType)
        {
            object service;

            _services.TryGetValue(serviceType, out service);

            return service;
        }

        public void RemoveService (Type serviceType)
        {
            _services.Remove(serviceType);
        }
    }
}
