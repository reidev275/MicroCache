using System;

namespace MicroCache
{
    public interface ICached<T>
    {
        T Object { get; set; }
        DateTime LastAccessed { get; set; }
    }
}
