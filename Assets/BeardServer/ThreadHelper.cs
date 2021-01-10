using System;
using System.Collections.Generic;

namespace BeardServer
{
    public class ThreadHelper
    {
        private static List<Action> mActionsToExecuteOnMainThread = new List<Action>();
        private static bool bAnyActionsToExecute = false;

        public static void ExecuteOnMainThread(Action action)
        {
            if (action == null)
                return;

            lock(mActionsToExecuteOnMainThread)
            {
                mActionsToExecuteOnMainThread.Add(action);
                bAnyActionsToExecute = true;
            }
        }

        public static void UpdateMain()
        {
            if (bAnyActionsToExecute)
            {
                List<Action> copy = new List<Action>();
                lock(mActionsToExecuteOnMainThread)
                {
                    copy.AddRange(mActionsToExecuteOnMainThread);
                    mActionsToExecuteOnMainThread.Clear();
                    bAnyActionsToExecute = false;
                }

                foreach (var action in copy)
                {
                    action?.Invoke();
                }
            }
        }
    }
}