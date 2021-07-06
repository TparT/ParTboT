using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;

namespace YarinGeorge.Utilities.Extra
{
    public class AppPerformance
    {
        public static async Task<(string CPU, string RAM)> CurrentAppPerfomanceStats()
        {
            var process = Process.GetCurrentProcess();

            var startTime = DateTime.UtcNow;
            var startCpuUsage = process.TotalProcessorTime;
            await Task.Delay(500);

            var endTime = DateTime.UtcNow;
            var endCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;
            var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
            var totalMsPassed = (endTime - startTime).TotalMilliseconds;
            var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);
            var CpuUsage = (cpuUsageTotal * 100).ToString();


            return (CPU: CpuUsage, RAM: "0");
        }
    }

    public enum Return
    {
        CPU,
        RAM,
        UpTime
    }
}