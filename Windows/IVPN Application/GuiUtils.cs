using System;
using System.Windows;
using System.Windows.Threading;

namespace IVPN
{
    public static class GuiUtils
    {
        public static void InvokeInGuiThread(DependencyObject window, Action action)
        {
            if (window == null)
                return;

            // Checking if this thread has access to the object.
            if (window.Dispatcher.CheckAccess())
                action();
            else
                window.Dispatcher.BeginInvoke(action, DispatcherPriority.Normal);
        }

        public static bool IsInvokedInGuiThread(DependencyObject window, Action action)
        {
            if (window == null)
                return false;

            // Checking if this thread has access to the object.
            if (window.Dispatcher.CheckAccess())
                return false;

            window.Dispatcher.BeginInvoke(action, DispatcherPriority.Normal);
            return true;
        }
    }
}
