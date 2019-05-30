using System;
using System.Windows.Forms;

namespace Monitor.Windows
{
    public static class FormExtensions
    {
        public static T Invoke<T>(this Form form, Func<T> action)
        {
            T r = default(T);
            form.Invoke((MethodInvoker) (() => { r = action(); }));
            return r;
        }

        public static void Invoke<T>(this Form form, Action action)
        {
            form.Invoke((MethodInvoker) (() => { action(); }));
        }
    }
}