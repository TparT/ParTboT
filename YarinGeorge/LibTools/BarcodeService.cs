using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXing;
using ZXing.QrCode;

namespace YarinGeorge.LibTools
{
    public class BarcodeService
    {
        private BarcodeWriter BW { get; set; }
        private BarcodeReader BR { get; set; }

        public BarcodeService(BarcodeFormat Format = BarcodeFormat.QR_CODE, int Width = 400, int Height = 400)
        {
            BW = new BarcodeWriter
            {
                // QR code format/type
                Format = Format,
                // QR code image output size
                Options = new QrCodeEncodingOptions
                {
                    Width = Width,
                    Height = Height
                }
            };

            BR = new BarcodeReader();
        }

        public MemoryStream GenerateBarcodeImage(string contents, BarcodeFormat Format = BarcodeFormat.QR_CODE, int Width = 400, int Height = 400)
        {
            BW.Format = Format;
            BW.Options.Width = Width;
            BW.Options.Height = Height;

            var ms = new MemoryStream();
            BW.Write(contents).Save(ms, ImageFormat.Png);
            ms.Position = 0;

            return ms;
        }

        public string ReadBarcode(Stream stream)
            => BR.Decode(new Bitmap(stream)).Text;
    }
}
