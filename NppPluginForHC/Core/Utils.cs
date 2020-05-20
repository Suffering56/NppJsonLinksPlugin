using System;
using System.Threading;

namespace NppPluginForHC.Core
{
    public static class Utils
    {
        public static void ExecuteDelayed(Action runnable, int delay)
        {
            new Thread(o =>
                {
                    Thread.Sleep(delay);
                    runnable.Invoke();
                }
            ).Start();
        }
    }
}