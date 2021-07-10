using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Timers;

namespace YarinGeorge.Utilities.Extensions
{
    /// <summary>
    /// Just some more extension methods made by yours truly ~Yarin George.
    /// </summary>
    public static class ExtraExtensions
    {
        /// <summary>
        /// Restarts the timer.
        /// </summary>
        /// <param name="timer">The timer to reset.</param>
        public static void Reset(this Timer timer)
        {
            timer.Stop();
            timer.Start();
        }

        /// <summary>
        /// Starts multiple processes.
        /// </summary>
        /// <param name="Infos">The processes infos to start the processes.</param>
        /// <param name="WaitForExit">Wait for the processes to exit.</param>
        /// <returns>The started processes.</returns>
        public static async Task<List<Process>> StartProcesses(this List<ProcessStartInfo> Infos, bool WaitForExit)
        {
            List<Process> processesBuffer = new List<Process>();
            await Task.Run(() => {
                foreach (ProcessStartInfo Info in Infos)
                {
                    Process process = Process.Start(Info);
                    if (WaitForExit)
                        process.WaitForExit();

                    processesBuffer.Add(process);
                }

                return Task.CompletedTask;
            });

            return processesBuffer;
        }
    }
}
