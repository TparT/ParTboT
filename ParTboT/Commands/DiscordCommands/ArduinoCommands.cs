using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.IO.Ports;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ParTboT.Commands
{
    [Group("led")]
    [Description("Controls the ON/OFF state of the arduino LED.")]
    [RequireOwner]
    public class ArduinoCommands : BaseCommandModule
    {
        SerialPort port;
        private void SetupPort()
        {
            if (port == null)
            {
                port = new SerialPort("COM4", 9600);//Set your board COM
                port.Open();
            }
        }

        [Command("on")]
        [Description("Sets the power state of the LED to ON.")]
        public async Task On(CommandContext ctx)
        {
            SetupPort();
            PortWrite("1");

            await ctx.Channel.SendMessageAsync("The LED is now ON! :blue_circle:").ConfigureAwait(false);
        }

        [Command("off")]
        [Description("Sets the power state of the LED to off.")]
        public async Task Off(CommandContext ctx)
        {
            SetupPort();
            PortWrite("0");

            await ctx.Channel.SendMessageAsync("The LED is now OFF! :white_circle:").ConfigureAwait(false);
        }

        private void PortWrite(string message) =>
            port.Write(message);
    }
}
