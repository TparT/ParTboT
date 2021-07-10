using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Figgle;
using EasyConsole;

namespace YarinGeorge.Utilities.Debugging
{
    public static class Debugging
    {
        public static async Task StatsTrack()
        {
            DateTime startTime = DateTime.Now;
            var AppName = Process.GetCurrentProcess().ProcessName;
        Again:
            var delta = DateTime.Now - startTime;

            var Months = $"{delta.Days / 30:0#}";
            var Days = $"{delta.Days:0#}";
            var Hours = $"{delta.Hours:0#}";
            var Minutes = $"{delta.Minutes:0#}";
            var Seconds = $"{delta.Seconds:0#}";
            //var Performance = await AppPerformance.CurrentAppPerfomanceStats();

            //if (Timer.Hour.Equals(23) && Timer.Minute.Equals(59) && Timer.Second.Equals(59)) T.AddDays(1);
            Console.Title =
                $"[{AppName}] - [Uptime -> Up since: {startTime} | Up for: {Months} months, {Days} days, {Hours} hours, {Minutes} minutes, {Seconds} seconds] [Time today (now): {DateTime.Now}]";// | [Performance -> CPU: {Performance.CPU:0#}% , RAM: {Performance.RAM:0#}Mb]";
            await Task.Delay(1000);
            goto Again;
        }

        public static void OutputBigExceptionError(this Exception exception)
        {
            StringBuilder ExceptionErrorMessage = new StringBuilder();
            ExceptionErrorMessage.Append($"\n{FiggleFonts.Standard.Render("[   ERROR   ]")}\n");
            ExceptionErrorMessage.Append($"[{DateTime.Now}]");
            if (exception.HResult != null) ExceptionErrorMessage.Append($" | [Code ({exception.HResult})]\n\n");
            if (exception.Message != null) ExceptionErrorMessage.AppendLine($"Exception message: {exception.Message}\n");
            if (exception.InnerException != null) ExceptionErrorMessage.AppendLine($"\nInnerException message: {exception.InnerException}\n");
            if (exception.TargetSite != null) ExceptionErrorMessage.AppendLine($"Method that threw the exception:\n{exception.TargetSite}\n");

            Output.WriteLine(ConsoleColor.Red, $"{ExceptionErrorMessage}");
        }

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

        public static void OutputBigError(string Contents = "") =>
            Output.WriteLine(ConsoleColor.Red, $"{FiggleFonts.Standard.Render($"[   ERROR   ]")}\n{Contents}");

        public static void WriteFiggleColor(this string String, FiggleFont Font, ConsoleColor ConsoleColor)
            => Output.WriteLine(ConsoleColor, Font.Render(String));
    }
}
