using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ParTboT.Commands.TextCommands
{
    [Group("qr")]
    public class QrCommands : BaseCommandModule
    {
        public ServicesContainer Services { private get; set; }

        [Command("make")]
        [Aliases("n", "new", "create", "generate", "encode")]
        [Description("Generates a QR code from link or just text.")]
        public async Task QrMake(CommandContext ctx, [RemainingText, Description("The contents of the QR code")] string contents)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);

            MemoryStream ms = Services.BarcodeService.GenerateBarcodeImage(contents);
            ms.Position = 0;
            await ctx.RespondAsync(new DiscordMessageBuilder().WithFile("Barcode.png", ms)).ConfigureAwait(false);

            ms.SetLength(0);
            ms.Dispose();
            ms.Close();
            #region old way
            /*System.IO.DirectoryInfo WorkingPath = new DirectoryInfo($"C:\\Users\\yarin\\Documents\\Visual studio projects\\Discord\\C# Discord bots\\GogyBot_Alpha\\GogyBot Alpha\\GogyBot Alpha\\TempFiles\\QR\\Generated\\");

            int NumberOfFiles = 0;
            foreach (FileInfo file in WorkingPath.GetFiles())
            {
                NumberOfFiles++;
            }

            // file name
            string FileName = "QRcode" + NumberOfFiles; // E.g. "QRcode1"

            // set the barcode output image file format
            string OutputFormat = ".png"; // E.g. ".png"

            // file full name
            string FullFileName = FileName + OutputFormat; // E.g. "QRcode1.png"

            // file full path
            string OutputFile = WorkingPath + FullFileName; // E.g. ~/images/QRcodeImages/QRcode1.png

            // write text and generate a 2-D barcode as a bitmap
            barcodeWriter.Write(contents).Save(OutputFile, ImageFormat.Png);

            // Respond with the QR code
            await ctx.RespondWithFileAsync(OutputFile).ConfigureAwait(false);

            // wait 3.5 seconds before deleting the generated files
            System.Threading.Thread.Sleep(3500);

            // delete any generated file that was created after the time set above
            foreach (FileInfo file in WorkingPath.GetFiles())
            {
                file.Delete();
            }*/
            #endregion
        }

        [Command("read")]
        [Aliases("r")]
        [Description("Reads a QR code from link or attachment.")]
        public async Task QrRead(CommandContext ctx, string Link = null)
        {
            string BarcodeText = Services.BarcodeService.ReadBarcode
                ((await WebRequest.Create(Link ?? ctx.Message.Attachments[0].Url).GetResponseAsync().ConfigureAwait(false)).GetResponseStream());

            await ctx.RespondAsync(BarcodeText).ConfigureAwait(false);
        }
    }
}
