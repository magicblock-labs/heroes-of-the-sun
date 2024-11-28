using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

//contains functionality classically implemented in a context.
//combined in attempt to simplify

namespace Utils.Injection
{
    public class Injector
    {
        private const BindingFlags BindingFlags = System.Reflection.BindingFlags.DeclaredOnly |
                                                  System.Reflection.BindingFlags.NonPublic |
                                                  System.Reflection.BindingFlags.Instance;

        private static Injector _instance;

        public static Injector Instance
        {
            get
            {
                if (_instance == null)
                {
                    Setup(typeof(Injector).Assembly);
                }

                return _instance;
            }
        }

        private readonly Dictionary<string, object> _cache = new Dictionary<string, object>();

        private readonly Dictionary<Type, List<FieldInfo>> _injectionPoints = new Dictionary<Type, List<FieldInfo>>();

        public static void Setup(Assembly assembly)
        {
            _instance = new Injector();

            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (a != assembly)
                    continue;

                a.GetTypes().ToList().ForEach(Instance.ProcessType);
            }
        }

        private void ProcessType(Type type)
        {
            if (type.IsAbstract)
                return;

            ProcessInjectionPoints(type, type);
        }

        private void ProcessInjectionPoints(Type owner, Type type)
        {
            foreach (var member in type.GetFields(BindingFlags))
            {
                if (member.GetCustomAttributes(typeof(Inject), true).Length == 0)
                    continue;

                if (_injectionPoints.ContainsKey(owner))
                    _injectionPoints[owner].Add(member);
                else
                    _injectionPoints.Add(owner, new List<FieldInfo> { member });
            }

            if (type.BaseType != null)
                ProcessInjectionPoints(owner, type.BaseType);
        }

        public void Resolve(object obj, int ownerId = 0)
        {
            if (!_injectionPoints.ContainsKey(obj.GetType()))
                return;

            foreach (var member in _injectionPoints[obj.GetType()])
            {
                var val = ResolveValue(member, obj, ownerId);
                member.SetValue(obj, val);
            }
        }

        private object ResolveValue(FieldInfo member, object obj, int ownerId)
        {
            if (member.FieldType.GetCustomAttributes(typeof(Singleton), false).Length > 0)
                return GetValue(member.FieldType);

            var resolveValue = GetValue(member.FieldType, ownerId);

            return resolveValue;
        }

        internal object GetValue(Type type, int id = 0)
        {
            var key = id != 0 ? $"{type}:{id}" : type.ToString();

            if (!_cache.ContainsKey(key))
            {
                var instance = Activator.CreateInstance(type);

                _cache.Add(key, instance);

                Resolve(instance);
            }

            return _cache[key];
        }
    }
}