using System;
using System.Collections.Generic;
using Treefrog.Framework;
using Treefrog.Presentation;

namespace Treefrog.Extensibility
{
    public abstract class Presenter : IDisposable
    {
        private Dictionary<Type, Action<Presenter>> _attachActions;
        private Dictionary<Type, Action<Presenter>> _detachActions;

        protected Presenter (PresenterManager pm)
        {
            Manager = pm;

            _attachActions = new Dictionary<Type, Action<Presenter>>();
            _detachActions = new Dictionary<Type, Action<Presenter>>();

            Manager.InstanceRegistered += HandlePresenterRegistered;
            Manager.InstanceUnregistered += HandlePresenterUnregistered;
        }

        public void Dispose ()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose (bool disposing)
        {
            if (disposing) {
                Manager.InstanceRegistered -= HandlePresenterRegistered;
                Manager.InstanceUnregistered -= HandlePresenterUnregistered;

                // Run all registered detach events on disposed object
                foreach (var kv in _detachActions) {
                    Presenter instance = Manager.Lookup(kv.Key);
                    if (instance != null)
                        kv.Value(instance);
                }

            }
        }

        protected PresenterManager Manager { get; private set; }

        protected void OnAttach<T> (Action<T> action)
            where T : Presenter
        {
            _attachActions.Add(typeof(T), p => action(p as T));

            T target = Manager.Lookup<T>();
            if (target != null)
                action(target);
        }

        protected void OnDetach<T> (Action<T> action)
            where T : Presenter
        {
            _detachActions.Add(typeof(T), p => action(p as T));
        }

        private void HandlePresenterRegistered (object sender, InstanceRegistryEventArgs<Presenter> e)
        {
            if (_attachActions.ContainsKey(e.Type) && e.Type != GetType())
                _attachActions[e.Type](e.Instance);
        }

        private void HandlePresenterUnregistered (object sender, InstanceRegistryEventArgs<Presenter> e)
        {
            if (_detachActions.ContainsKey(e.Type) && e.Type != GetType())
                _detachActions[e.Type](e.Instance);
        }
    }
}
