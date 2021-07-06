using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace YarinGeorge.Utilities
{
    public class MoreTools
    {
        public static async Task<Process[]> StartProcesses(List<ProcessStartInfo> infos, bool waitForExit)
        {
            ArrayList processesBuffer = new ArrayList();
            await Task.Run(async () =>
            {
                foreach (ProcessStartInfo info in infos)
                {
                    Process process = Process.Start(info);

                    if (waitForExit)
                    {
                        process.WaitForExit();
                    }

                    processesBuffer.Add(process);
                }
            });


            return (Process[]) processesBuffer.ToArray(typeof(Process));
        }
    }
}