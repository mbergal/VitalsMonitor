using System;

namespace Monitor.Windows
{
    public interface IWithEffect<T>
    {
        Action<Action<Action<T>>> Effect { get; set; }
    }
}