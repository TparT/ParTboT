using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Drawing.Imaging;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using ZXing;
using ZXing.QrCode;


namespace ParTboT.Commands
{
    [Group("invite")]
    public class InviteCommands : BaseCommandModule
    {
        [Command("inviteqr")]
        //[Aliases("")]
        [Description("Generates a QR code from link or just text.")]
        public async Task InviteQR(CommandContext ctx, [RemainingText, Description("The contents of the QR code")] string contents)
        {
            await ctx.TriggerTypingAsync();

            // instantiate a writer object
            var barcodeWriter = new BarcodeWriter
            {
                // QR code format/type
                Format = BarcodeFormat.QR_CODE,
                // QR code image output size
                Options = new QrCodeEncodingOptions
                {
                    Width = 400,
                    Height = 400
                }
            };

            var ms = new MemoryStream();
            barcodeWriter.Write(contents).Save(ms, ImageFormat.Png);

            // If you're going to read from the stream, you may need to reset the position to the start
            ms.Position = 0;

            var Message = new DiscordMessageBuilder();
            Message.WithFile("BarcodeFileFromStream.png", ms);


            await ctx.RespondAsync(Message).ConfigureAwait(false);

            //System.IO.MemoryStream.Reset

            byte[] buffer = ms.GetBuffer();
            Array.Clear(buffer, 0, buffer.Length);
            ms.Position = 0;
            ms.SetLength(0);
            ms.Dispose();
            ms.Close();
        }
    }

    class subcommand : InviteCommands
    {
        [Command("subc")]
        [Description("Says the name of the bot's developer")]
        public async Task SubC(CommandContext ctx)
        {
            await ctx.RespondAsync("Im a sub sub command").ConfigureAwait(false);
        }
    }
}
