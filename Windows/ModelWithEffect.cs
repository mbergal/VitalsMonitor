using System;

namespace Monitor.Windows
{
    public class ModelWithEffect<T> : IWithEffect<T>
    {
        public Action<Action<Action<T>>> Effect { get; set; }
    }
}