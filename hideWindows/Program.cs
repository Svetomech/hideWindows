using Svetomech.Utilities;
using Svetomech.Utilities.Types;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using static Svetomech.Utilities.SimpleProcess;

namespace hideWindows
{
    static class Program
    {
        [STAThread]
        static void Main(string[] processesToHideNames)
        {
            var settings = Properties.Settings.Default;
            if (settings.UpgradeRequired)
            {
                settings.Upgrade();
                settings.UpgradeRequired = false;
            }

            bool firstRun = (settings.HiddenProcessesWindows == null);

            string[] processesToShowNames = null;
            if (!firstRun)
            {
                processesToShowNames = new string[settings.HiddenProcessesWindows.Keys.Count];
                settings.HiddenProcessesWindows.Keys.CopyTo(processesToShowNames, 0);
            }

            StringDictionary hiddenProcessesWindows = new StringDictionary();

            foreach (string procToHideName in processesToHideNames)
            {
                Window[] windowsToShowOrHide = null;

                if (processesToShowNames?.Length > 0)
                {
                    foreach (string procToShowName in processesToShowNames)
                    {
                        if (!procToShowName.Equals(Path.GetFileNameWithoutExtension(procToHideName), StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        string[] windowsToShowHandles = settings.HiddenProcessesWindows[procToShowName].Split(' ');

                        windowsToShowOrHide = WindowFactory.Populate.FromHandles(windowsToShowHandles);
                    }
                }

                windowsToShowOrHide = windowsToShowOrHide ?? GetVisibleWindows(procToHideName);

                List<string> hiddenWindowsHandles = new List<string>();

                foreach (Window window in windowsToShowOrHide)
                {
                    if (!window.IsShown)
                        window.Show();
                    else
                    {
                        window.Hide();
                        hiddenWindowsHandles.Add(window.ToString());
                    }
                }

                if (hiddenWindowsHandles?.Count > 0)
                {
                    hiddenProcessesWindows[Path.GetFileNameWithoutExtension(procToHideName)] = String.Join(" ", hiddenWindowsHandles);
                }
            }

            settings.HiddenProcessesWindows = hiddenProcessesWindows;
            settings.Save();
        }
    }
}
