using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using EasyConsole;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Renci.SshNet;
using Serilog.Core;
using SshNet;

namespace ParTboT.Commands
{
    [Group("ssh"), Aliases("")]
    [Description("Commands for controlling linux machines using the SSH protocol.")]
    public class SSHCommands : BaseCommandModule
    {
        [Command("new")]
        [Description("Creates a new SSH connection")]
        public async Task NewSSH
            (

            CommandContext ctx,
            [Description("Host name OR Ip address")] string host,
            [Description("Connection port (Default port is usually 22)")] int port,
            [Description("Username to log as")] string username,
            [Description("Login password")] string password,
            [Description("The command to execute")] [RemainingText] string command
            
            )

        {

            await ctx.Message.DeleteAsync().ConfigureAwait(false);

            byte[] expectedFingerPrint = new byte[]
            {
                    0x66, 0x31, 0xaf, 0x00, 0x54, 0xb9, 0x87, 0x31,
                    0xff, 0x58, 0x1c, 0x31, 0xb1, 0xa2, 0x4c, 0x6b
            };

            // Setup Credentials and Server Information
            ConnectionInfo ConnNfo = new ConnectionInfo(host, port, username,
                new AuthenticationMethod[]{

                // Pasword based Authentication
                new PasswordAuthenticationMethod(username,password),

                // Key Based Authentication (using keys in OpenSSH Format)
                new PrivateKeyAuthenticationMethod(username,new PrivateKeyFile[]{
                    new PrivateKeyFile(@"",password)
                }),
                }
            );

            // Execute a (SHELL) Command - prepare upload directory
            using (var sshclient = new SshClient(ConnNfo))
            {
                sshclient.Connect();
                using (var CommandOutput = sshclient.RunCommand(command))
                {
                    Console.WriteLine(CommandOutput.ToString());

                    
                    CommandOutput.Execute();
                    await ctx.Channel.SendMessageAsync(
                        $"```" +
                        $"\n" +
                        $"Command> {CommandOutput.CommandText}" +
                        $"\n\n" +
                        $"Output:" +
                        $"\n\n" +
                        $"{CommandOutput.Result}" +
                        $"\n" +
                        $"```");
                    Output.WriteLine(CommandOutput.Result);                    
                    //Console.WriteLine("Return Value = {0}", output.ExitStatus);
                }
                sshclient.Disconnect();
            }
        }

        [Command("send")]
        [Description("Sends a command to the linux machine")]
        public async Task Send(CommandContext ctx, [Description("The command to send")] [RemainingText] string command)
        {
            /*client.Connect();
            var cmd = client.CreateCommand("echo 12345; echo 654321 >&2");
            var result = cmd.Execute();

            Console.Write(result);

            var reader = new StreamReader(cmd.ExtendedOutputStream);
            Console.WriteLine("DEBUG:");
            Console.Write(reader.ReadToEnd());*/
        }
    }
}
