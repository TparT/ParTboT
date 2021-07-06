using System.Drawing;
using System.IO;
using System.Net;
using System.Threading.Tasks;

using YarinGeorge.LibTools;

namespace YarinGeorge.Utilities.Converters
{
    public enum OutputType
    {
        /// <summary>
        /// The color code type that starts with # and then some other letters and numbers afterwards [E.g: #FF5733 (Orange)]
        /// </summary>
        Hex,
        /// <summary>
        /// The color code type that has 3 different values for each color its made of (Red, Green and Blue) [E.g: 255, 87, 51 (Orange)]
        /// </summary>
        RGB,
        
    }
    
    public class AverageImageColor
    {
        /// <summary>
        /// Asynchronously gets the average color code value of an image by a given image URL.
        /// </summary>
        /// <param name="ImageURL">The url of the image you want to get the average color from.</param>
        /// <param name="CodeType">The <see cref="OutputType"/> of the color code (Hex [#FF5733] or RGB [255,87,51]).</param>
        /// <returns></returns>
        public static async Task<string> GetAverageColorByImageUrlCodeAsync(string ImageURL, OutputType CodeType)
        {
            string color = string.Empty;
            Stream responseStream =
                (await WebRequest.Create(ImageURL).GetResponseAsync()
                .ConfigureAwait(false)).GetResponseStream();

            switch (CodeType)
            {
                case OutputType.Hex: color = ColorMath.getDominantColor(new Bitmap(responseStream)).Name; break;
                case OutputType.RGB: color = ColorMath.getDominantColor(new Bitmap(responseStream)).ToString(); break;
            }
            return color;
        }
        
        public static async Task<Color> GetAverageColorByImageUrlAsync(string ImageURL)
        {
            Stream responseStream =
                (await WebRequest.Create(ImageURL).GetResponseAsync()
                .ConfigureAwait(false)).GetResponseStream();

            Color color = ColorMath.getDominantColor(new Bitmap(responseStream));

            return color;
        }  
    }
}