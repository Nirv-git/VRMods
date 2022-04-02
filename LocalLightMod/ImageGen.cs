using System;
using MelonLoader;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Text;
using UnityEngine;

namespace LocalLightMod
{
    class ImageGen
    {
        /// https://stackoverflow.com/a/57223744
        public static Image DrawText(string text)
        {
            System.Drawing.Font font = Control.DefaultFont;
            try { font = new System.Drawing.Font("Arial", 60, System.Drawing.FontStyle.Bold); }
            catch { Main.Logger.Msg("You dont have Arial!"); }
            System.Drawing.Color textColor = System.Drawing.Color.White;
            System.Drawing.Color backColor = System.Drawing.Color.Black;

            //Create a dummy bitmap just to get a graphics object
            SizeF textSize;
            using (Image img = new Bitmap(1, 1))
            {
                using (System.Drawing.Graphics drawing = System.Drawing.Graphics.FromImage(img))
                {
                    //Measure the string to see how big the image needs to be
                    textSize = drawing.MeasureString(text, font);
                }
            }
            //Create a new image of the right size
            int max = Math.Max((int)textSize.Width, (int)textSize.Height); //Needs to be Square for the texture
            //Image retImg = new Bitmap((int)textSize.Width, (int)textSize.Height);
            Image retImg = new Bitmap(max, max);
            using (var drawing = System.Drawing.Graphics.FromImage(retImg))
            {
                //Paint the background
                drawing.Clear(backColor);
                //Create a brush for the text
                using (Brush textBrush = new SolidBrush(textColor))
                {
                    drawing.DrawString(text, font, textBrush, 0, retImg.Size.Height / 2);
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
