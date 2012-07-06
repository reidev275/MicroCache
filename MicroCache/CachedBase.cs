using System;

namespace MicroCache
{
    public class CachedBase<T> : ICached<T>
    {
        public T Object { get; set; }
        public DateTime LastAccessed { get; set; }

        public CachedBase(T obj)
        {
            LastAccessed = DateTime.Now;
            Object = obj;
        }
    } 
}
