using System.Collections.Generic;

namespace Utils.Signal
{
    public class Signal  //todo implement iSubuscribable visible to consumers (so they cant dispatch)
    {
        public delegate void Callback();

        private readonly List<Callback> _callbacks;

        public Signal()
        {
            _callbacks = new List<Callback>();
        }

        public void Dispatch()
        {
            for (var i = 0; i < _callbacks.Count; i++) _callbacks[i].Invoke();
        }

        public void Add(Callback callback)
        {
            _callbacks.Add(callback);
        }

        public void Remove(Callback callback)
        {
            _callbacks.Remove(callback);
        }
        
        public void Clear()
        {
            _callbacks.Clear();
        }
    }
    
    public class Signal<T>
    {
        public delegate void Callback(T p);

        private readonly List<Callback> _callbacks;

        public Signal()
        {
            _callbacks = new List<Callback>();
        }

        public void Dispatch(T p)
        {
            for (var i = 0; i < _callbacks.Count; i++) _callbacks[i].Invoke(p);
        }

        // Use this for initialization
        public void Add(Callback callback)
        {
            _callbacks.Add(callback);
        }

        // Update is called once per frame
        public void Remove(Callback callback)
        {
            _callbacks.Remove(callback);
        }
        
        public void Clear()
        {
            _callbacks.Clear();
        }
    }

    public class Signal<T1, T2>
    {
        public delegate void Callback(T1 p1, T2 p2);

        private readonly List<Callback> _callbacks;

        public Signal()
        {
            _callbacks = new List<Callback>();
        }

        public void Dispatch(T1 p1, T2 p2)
        {
            for (var i = 0; i < _callbacks.Count; i++) _callbacks[i].Invoke(p1, p2);
        }

        // Use this for initialization
        public void Add(Callback callback)
        {
            _callbacks.Add(callback);
        }

        // Update is called once per frame
        public void Remove(Callback callback)
        {
            _callbacks.Remove(callback);
        }
        
        public void Clear()
        {
            _callbacks.Clear();
        }
    }
}