using CaptchaN;
using CaptchaN.Abstractions;
using CaptchaN.Factories;
using System.IO;
using System.Threading.Tasks;

namespace ParTboT.Services
{
    public class UserVerifications
    {
        private Painter _painter { get; set; }
        private PainterOption _painterOption { get; set; }

        public UserVerifications InitImageCAPTCHAGeneratorService()
        {
            DirectoryFontRandomerFactory fontRandomerFactory = new DirectoryFontRandomerFactory() { FontDir = new DirectoryInfo($@"C:\Users\yarin\Documents\DiscordBots\ParTboT\ParTboT\Assets\Fonts") };
            IFontRandomer fontRandomer = fontRandomerFactory.CreateFontRandomer();

            ColorRandomer colorRandomer = new ColorRandomer();
            _painter = new Painter(fontRandomer, colorRandomer);
            _painterOption = new PainterOption
            {
                Height = 50,
                Width = 200,
                LineCount = 3
            };

            return this;
        }

        public async Task<byte[]> GenerateCAPTCHAImageAsync(string Code, PainterOption PainterOptions = null)
            => await _painter.GenerateImageAsync(Code, PainterOptions ?? _painterOption);
    }
}
