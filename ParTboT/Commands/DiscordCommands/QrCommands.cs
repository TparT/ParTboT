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
    [Group("qr")]
    public class QrCommands : BaseCommandModule
    {
        [Command("make")]
        [Aliases("new", "create", "generate", "encode")]
        [Description("Generates a QR code from link or just text.")]
        public async Task Qr(CommandContext ctx, [RemainingText, Description("The contents of the QR code")] string contents)
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

            await ctx.Channel.SendMessageAsync(Message).ConfigureAwait(false);

            byte[] buffer = ms.GetBuffer();
            Array.Clear(buffer, 0, buffer.Length);
            ms.Position = 0;
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
    }
}
