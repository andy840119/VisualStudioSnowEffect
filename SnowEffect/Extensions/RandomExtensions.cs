/*
The MIT License (MIT)

Copyright (c) 2015 Liam Morrow

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

*/using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ShowEffect.Extensions
{
    /// <summary>
    /// 靜態的隨機功能
    /// </summary>
    public static class RandomExtensions
    {
        public static int NextSignSwap(this Random random)
        {
            return random.Next(0, 2) == 1 ? 1 : -1;
        }
        public static Color NextColor(this Random random)
        {
            byte[] bytes = new byte[3];
            random.NextBytes(bytes);

            return Color.FromRgb(bytes[0], bytes[1], bytes[2]);
        }
    }

    /// <summary>
    /// 靜態的擴充功能
    /// </summary>
    public static class ShowExtension
    {
        /// <summary>
        /// 混合顏色
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <param name="precentageA"></param>
        /// <param name="precentageB"></param>
        /// <returns></returns>
        public static Color MixColor(Color OrigColor, Color AddingMixingColor, float precentageA, float precentageB)
        {

            Color returnColor = new Color();
            returnColor = OrigColor * precentageA + AddingMixingColor * precentageB;
            returnColor.A = OrigColor.A;
            return returnColor;
        }

        /// <summary>
        /// 建立雪的形狀
        /// 如果有圖片就使用圖片
        /// </summary>
        /// <param name="color"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static Image GetImage(Color color,float size)
        {
            Image image;

            if (SnowConfig.Image != null)//顯示載入的圖片，
            {
                image = new Image();
                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(SnowConfig.Image);
                
                IntPtr hBitmap = bmp.GetHbitmap();
                System.Windows.Media.ImageSource WpfBitmap = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

                image.Source = WpfBitmap;
                image.Width = size;
                image.Height = size;
                image.Stretch = System.Windows.Media.Stretch.Fill;
            }
            else//建立圓形圖片
            {
                Rect rect = new Rect(-(SnowConfig.SnowSize / 2), -(SnowConfig.SnowSize / 2), (SnowConfig.SnowSize / 2), (SnowConfig.SnowSize / 2));
                EllipseGeometry geometry = new EllipseGeometry(rect);
              
                var brush = new SolidColorBrush(color);
                var drawing = new GeometryDrawing(brush, null, geometry);
                drawing.Freeze();

                var drawingImage = new DrawingImage(drawing);
                drawingImage.Freeze();
                image = new Image
                {
                    Source = drawingImage,
                    Visibility = Visibility.Hidden
                };
            }
            return image;
        }

        /// <summary>
        /// 落下高度
        /// </summary>
        /// <returns></returns>
        public static double FallDownHeight(double upPosition, double downPosition)
        {
            //落下高度
            if (SystemConfig.FallDownHeightMode == SystemConfig.SnowFallDownHeightMode.Pixel)//如果是pixel模式，落下高度對應解析度
            {
                return + SystemConfig.SnowResolutionHeight;
            }
            else
            {
                return SystemConfig.SnowResolutionHeightPrecentage * (downPosition - upPosition);
            }
        }

        /// <summary>
        /// 落下時間
        /// </summary>
        /// <param name="upPosition"></param>
        /// <param name="downPosition"></param>
        /// <returns></returns>
        public static double AnimationTimeByFallDownTime(double upPosition, double downPosition)
        {
            //高度 / 每秒落下速度
            return FallDownHeight(upPosition, downPosition) / SnowConfig.FallDownSpeed;
        }
    }
}