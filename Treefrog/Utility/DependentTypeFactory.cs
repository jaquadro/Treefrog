﻿using System;
using System.Collections.Generic;

namespace Treefrog.Utility
{
    public class DependentTypeFactory<TKey, T>
        where T : class
    {
        private Dictionary<Type, Type> _registry = new Dictionary<Type, Type>();
        private Dictionary<Type, Func<T>> _activation1 = new Dictionary<Type, Func<T>>();
        private Dictionary<Type, Func<TKey, T>> _activation2 = new Dictionary<Type, Func<TKey, T>>();

        private static T DefaultActivation<TSub> ()
        {
            return Activator.CreateInstance(typeof(TSub)) as T;
        }

        protected Dictionary<Type, Type> Registry
        {
            get { return _registry; }
        }

        protected Type GetClosestType (Type type)
        {
            while (type != null) {
                if (_registry.ContainsKey(type))
                    return _registry[type];
                type = type.BaseType;
            }

            return null;
        }

        public T Create (Type keyType)
        {
            Type t = GetClosestType(keyType);
            if (t == null)
                return null;

            if (_activation1.ContainsKey(keyType))
                return _activation1[keyType]();
            else
                return Activator.CreateInstance(t) as T;
        }

        public T Create (TKey keyInst)
        {
            Type t = GetClosestType(keyInst.GetType());
            if (t == null)
                return null;

            if (_activation2.ContainsKey(keyInst.GetType()))
                return _activation2[keyInst.GetType()](keyInst);
            else if (_activation1.ContainsKey(keyInst.GetType()))
                return _activation1[keyInst.GetType()]();
            else
                return Activator.CreateInstance(t) as T;
        }

        public Type Lookup (Type keyType)
        {
            Type t;
            if (!_registry.TryGetValue(keyType, out t)) {
                return null;
            }

            return t;
        }

        public void Register<TSubKey, TSub> ()
            where TSubKey : TKey
            where TSub : T
        {
            Register<TSubKey, TSub>(DefaultActivation<TSub>);
        }

        public void Register<TSubKey, TSub> (Func<T> activation)
            where TSubKey : TKey
            where TSub : T
        {
            _registry[typeof(TSubKey)] = typeof(TSub);
            _activation1[typeof(TSubKey)] = activation;
        }

        public void Register<TSubKey, TSub> (Func<TKey, T> activation)
            where TSubKey : TKey
            where TSub : T
        {
            _registry[typeof(TSubKey)] = typeof(TSub);
            _activation2[typeof(TSubKey)] = activation;
        }

        public void Register (Type layerType, Type controlType, Func<T> activation)
        {
            _registry[layerType] = controlType;
            _activation1[layerType] = activation;
        }

        public void Register (Type layerType, Type controlType, Func<TKey, T> activation)
        {
            _registry[layerType] = controlType;
            _activation2[layerType] = activation;
        }
    }

    public class DependentTypeFactory<TKey, T, TParam> : DependentTypeFactory<TKey, T>
        where T : class
    {
        private Dictionary<Type, Func<TParam, T>> _activation1 = new Dictionary<Type, Func<TParam, T>>();
        private Dictionary<Type, Func<TKey, TParam, T>> _activation2 = new Dictionary<Type, Func<TKey, TParam, T>>();

        public T Create (Type keyType, TParam param)
        {
            Type t = GetClosestType(keyType);
            if (t == null)
                return null;

            if (_activation1.ContainsKey(keyType))
                return _activation1[keyType](param);
            else
                return Activator.CreateInstance(t) as T;
        }

        public T Create (TKey keyInst, TParam param)
        {
            Type t = GetClosestType(keyInst.GetType());
            if (t == null)
                return null;

            if (_activation2.ContainsKey(keyInst.GetType()))
                return _activation2[keyInst.GetType()](keyInst, param);
            else if (_activation1.ContainsKey(keyInst.GetType()))
                return _activation1[keyInst.GetType()](param);
            else
                return Activator.CreateInstance(t) as T;
        }

        public void Register<TSubKey, TSub> (Func<TParam, T> activation)
            where TSubKey : TKey
            where TSub : T
        {
            Registry[typeof(TSubKey)] = typeof(TSub);
            _activation1[typeof(TSubKey)] = activation;
        }

        public void Register<TSubKey, TSub> (Func<TKey, TParam, T> activation)
            where TSubKey : TKey
            where TSub : T
        {
            Registry[typeof(TSubKey)] = typeof(TSub);
            _activation2[typeof(TSubKey)] = activation;
        }

        public void Register (Type layerType, Type controlType, Func<TParam, T> activation)
        {
            Registry[layerType] = controlType;
            _activation1[layerType] = activation;
        }

        public void Register (Type layerType, Type controlType, Func<TKey, TParam, T> activation)
        {
            Registry[layerType] = controlType;
            _activation2[layerType] = activation;
        }
    }
}
