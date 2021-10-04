using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using EasyConsole;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ParTboT.Commands.TextCommands
{
    [Group("code")]
    public class CodeCommands : BaseCommandModule
    {
        private static readonly Encoding LocalEncoding = Encoding.UTF8;

        [Command("python")]
        [Aliases("py")]
        [Description("Runs the given Python code.")]

        public async Task RunPython(CommandContext ctx, [Description("[The Python code]")][RemainingText] string PythonCode)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);

            //string Output = null;

            try
            {
                try
                {
                    //py.Execute("print ('From IronPython => hello world!')"); //Nzg4MzM2OTYzMzQ2MTA0MzMw.X9iCAg.BYoTfiz6WKOuVWs1ZU38IRYwAbQ
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message.ToString());
                }

                ProcessStartInfo p = new ProcessStartInfo();
                p.FileName = @"C:\Users\yarin\AppData\Local\Programs\Python\Python39\python.exe"; //cmd process
                p.Arguments = $"print (\"hello there csharp code!\")"; //args is path to .py file and any cmd line args
                p.UseShellExecute = false;
                p.RedirectStandardOutput = true;
                using (Process process = Process.Start(p))
                {
                    using (StreamReader reader = process.StandardOutput)
                    {
                        string result = reader.ReadToEnd();

                        Output.WriteLine(result);

                        await ctx.RespondAsync
                        ($"```\n" +
                        $"{result}" +
                        $"\n```")
                        .ConfigureAwait(false);

                        //Output.WriteLine
                        //    (cmd.StandardOutput.ReadToEnd());


                    }
                }



                /*await ctx.RespondAsync
                    (

                        $"**__Results:__**" +
                        $"\n```py\n" +
                        $"{Output}" +
                        $"\n```\n"

                    ).ConfigureAwait(false);*/
            }
            catch (Exception ex)
            {
                //Output = LocalEncoding.GetString(errors.ToArray());

                await ctx.RespondAsync
                    (

                        $"**__ERRORS:__**" +
                        $"\n```py\n" +
                        $"{ex.ToString()}" +
                        $"\n```\n"

                    ).ConfigureAwait(false);

            }
        }
    }
}
