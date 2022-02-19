using System;
using MelonLoader;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace FLuxMod
{
    class ImageGen
    {
        /// <summary>
        /// Creates an image containing the given text.
        /// NOTE: the image should be disposed after use.
        /// </summary>
        /// <param name="text">Text to draw</param>
        /// <param name="fontOptional">Font to use, defaults to Control.DefaultFont</param>
        /// <param name="textColorOptional">Text color, defaults to Black</param>
        /// <param name="backColorOptional">Background color, defaults to white</param>
        /// <param name="minSizeOptional">Minimum image size, defaults the size required to display the text</param>
        /// <returns>The image containing the text, which should be disposed after use</returns>
        /// https://stackoverflow.com/a/57223744
        //public static Image DrawText(string text, Font fontOptional = null, Color? textColorOptional = null, Color? backColorOptional = null, Size? minSizeOptional = null)
        public static Image DrawText(string text, Size? minSizeOptional = null, Image baseImage = null)
        {
            System.Drawing.Font fontOptional = Control.DefaultFont;
            try { fontOptional = new System.Drawing.Font("Arial", 30, System.Drawing.FontStyle.Bold); }
            catch { fontOptional = new System.Drawing.Font(FontFamily.GenericSansSerif, 30, System.Drawing.FontStyle.Bold); Main.Logger.Msg("You dont have Arial!"); }
            System.Drawing.Color? textColorOptional = System.Drawing.Color.FromArgb(200, 200, 200);
            System.Drawing.Color? backColorOptional = System.Drawing.Color.Transparent;

            Image retImg = null;
            bool usingBaseImage = false;
            if (baseImage != null)
            {
                usingBaseImage = true;
                retImg = new Bitmap(baseImage);
                //retImg.Save("Image2.bmp");
            }

            System.Drawing.Font font = Control.DefaultFont;
            if (fontOptional != null) font = fontOptional;
            System.Drawing.Color textColor = System.Drawing.Color.Black;
            if (textColorOptional != null) textColor = (System.Drawing.Color)textColorOptional;
            System.Drawing.Color backColor = System.Drawing.Color.White;
            if (backColorOptional != null) backColor = (System.Drawing.Color)backColorOptional;
            Size minSize = Size.Empty;
            if (minSizeOptional != null) minSize = (Size)minSizeOptional;

            //Create a dummy bitmap just to get a graphics object
            SizeF textSize;
            using (Image img = new Bitmap(1, 1))
            {
                using (System.Drawing.Graphics drawing = System.Drawing.Graphics.FromImage(img))
                {
                    //Measure the string to see how big the image needs to be
                    textSize = drawing.MeasureString(text, font);
                    if (!minSize.IsEmpty)
                    {
                        textSize.Width = textSize.Width > minSize.Width ? textSize.Width : minSize.Width;
                        textSize.Height = textSize.Height > minSize.Height ? textSize.Height : minSize.Height;
                    }
                }
            }
            //Create a new image of the right size
            int max = Math.Max((int)textSize.Width, (int)textSize.Height); //Needs to be Square for the texture
            //Main.Logger.Msg($"W:{textSize.Width} H:{textSize.Height}");
            //max = 472;
            //Image retImg = new Bitmap((int)textSize.Width, (int)textSize.Height);
            if (!usingBaseImage) retImg = new Bitmap(max, max);
            using (var drawing = System.Drawing.Graphics.FromImage(retImg))
            {
                //Paint the background
                if (!usingBaseImage) drawing.Clear(backColor);
                //Create a brush for the text
                using (Brush textBrush = new SolidBrush(textColor))
                {
                    if (usingBaseImage)
                    {
                        StringFormat drawFormat = new StringFormat();
                        drawFormat.Alignment = StringAlignment.Center;
                        drawing.DrawString(text, font, textBrush, retImg.Size.Width / 2, 0, drawFormat);
                    }
                    else
                        drawing.DrawString(text, font, textBrush, 0, 0);
                    drawing.Save();
                }
            }
            //retImg.Save("Image.bmp");
            return retImg;
        }

        public static byte[] ImageToPNG(Image img)
        {
            using (var stream = new MemoryStream())
            {
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }
    }
}
