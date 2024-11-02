using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace alga.DB
{
    internal class Capcha
    {
        private static readonly Random random = new Random();

        public BitmapImage GenerateCaptchaBitmapImage(out string captchaText)
        {
            Bitmap bitmap = GenerateCaptchaImage(out captchaText);
            BitmapImage bitmapImage = new BitmapImage();
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
            }

            return bitmapImage;
        }

        private Bitmap GenerateCaptchaImage(out string captchaText)
        {
            int length = random.Next(4, 8);
            captchaText = GenerateRandomText(length);
            int width = 180;
            int height = 80;
            Bitmap bitmap = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.LightGray);
                AddNoise(g, width, height);
                DrawCaptchaText(g, captchaText, width, height);
            }
            return bitmap;
        }

        private string GenerateRandomText(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            char[] text = new char[length];
            for (int i = 0; i < length; i++)
            {
                text[i] = chars[random.Next(chars.Length)];
            }
            return new string(text);
        }

        private void AddNoise(Graphics g, int width, int height)
        {
            for (int i = 0; i < 100; i++)
            {
                int x = random.Next(width);
                int y = random.Next(height);
                g.FillRectangle(Brushes.DarkGray, x, y, 1, 1);
            }

            for (int i = 0; i < 10; i++)
            {
                int x1 = random.Next(width);
                int y1 = random.Next(height);
                int x2 = random.Next(width);
                int y2 = random.Next(height);
                g.DrawLine(Pens.Gray, x1, y1, x2, y2);
            }
        }

        private void DrawCaptchaText(Graphics g, string captchaText, int width, int height)
        {
            int x = 10;
            for (int i = 0; i < captchaText.Length; i++)
            {
                int fontSize = random.Next(18, 25);
                Color color = Color.FromArgb(random.Next(50, 255), random.Next(50, 255), random.Next(50, 255));
                using (Font font = new Font("Arial", fontSize, System.Drawing.FontStyle.Bold))
                using (Brush brush = new SolidBrush(color))
                {
                    float angle = random.Next(-20, 20);
                    g.TranslateTransform(x, height / 2);
                    g.RotateTransform(angle);

                    g.DrawString(captchaText[i].ToString(), font, brush, 0, 0, StringFormat.GenericDefault);
                    g.ResetTransform();
                    x += fontSize - 5;
                }
            }
        }
    }
}
