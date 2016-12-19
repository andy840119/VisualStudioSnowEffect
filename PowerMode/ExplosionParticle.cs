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
*/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using EnvDTE;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using PowerMode.Extensions;

namespace PowerMode
{
    public class ExplosionParticle
    {
       

        [ThreadStatic]
        private static Random _random;

        private readonly DTE _service;
        private readonly Action<ExplosionParticle> _afterExplode;
        private readonly IAdornmentLayer adornmentLayer;

        private static Rect _rect = new Rect(-5, -5, 5, 5);

        private static EllipseGeometry geometry = new EllipseGeometry(_rect);

        private Image _image;

        //往左邊的動畫
        private DoubleAnimation _leftAnimation;
        //往上飄移的動畫
        private DoubleAnimation _topAnimation;

        private DoubleAnimation _opacityAnimation;
        private double _iterations;
        private uint _optionsVersion = 0;


        private static int ParticleCount { get; set; }

        private static Random Random
        {
            get
            {
                if (_random == null)
                {
                    _random = new Random();
                }
                return _random;
            }
        }

        public ExplosionParticle(IAdornmentLayer adornment, DTE service, Action<ExplosionParticle> afterExplode)
        {
            adornmentLayer = adornment;
            _service = service;
            _afterExplode = afterExplode;
            InitializeOptions();
        }

        private void InitializeOptions()
        {
            
            Color brushColor = SnowConfig.Color;
            if (SnowConfig.MixGetColorFromEnvironment>0)
            {
                var svc = Package.GetGlobalService(typeof (SVsUIShell)) as IVsUIShell5;
                brushColor = ColorExtension.MixColor(brushColor , (svc.GetThemedWPFColor(EnvironmentColors.PanelTextColorKey)) ,(1- SnowConfig.MixGetColorFromEnvironment), SnowConfig.MixGetColorFromEnvironment);
            }
            else if (SnowConfig.MixRandomColor>0)
            {
                brushColor = ColorExtension.MixColor(brushColor, (Random.NextColor()), (1 - SnowConfig.MixRandomColor), SnowConfig.MixRandomColor);
            }
            else
            {
                brushColor = SnowConfig.Color;
            }

            //設定雪的大小
            _rect = new Rect(-(SnowConfig.SnowSize / 2), -(SnowConfig.SnowSize / 2), (SnowConfig.SnowSize / 2), (SnowConfig.SnowSize / 2));
            geometry = new EllipseGeometry(_rect);

            var brush = new SolidColorBrush(brushColor);
            var drawing = new GeometryDrawing(brush, null, geometry);
            drawing.Freeze();

            var drawingImage = new DrawingImage(drawing);
            drawingImage.Freeze();

            _image = new Image
            {
                Source = drawingImage,
                Visibility = Visibility.Hidden
            };

            // Add the image to the adornment layer and make it relative to the viewport
            adornmentLayer.AddAdornment(AdornmentPositioningBehavior.ViewportRelative,
                null,
                null,
                _image,
                null);

            _iterations = SnowConfig.StartAlpha / SnowConfig.AlphaRemoveAmount;

            //算出時間
            var timeSpan = TimeSpan.FromMilliseconds(TypingConfig.SnowFallDelay * _iterations);

            _leftAnimation = new DoubleAnimation();

            _topAnimation = new DoubleAnimation();

            _topAnimation.EasingFunction = new BackEase {Amplitude = SnowConfig.Gravity * 35};

            _opacityAnimation = new DoubleAnimation();
            _opacityAnimation.From = SnowConfig.StartAlpha;
            _opacityAnimation.To = SnowConfig.StartAlpha - (_iterations * SnowConfig.AlphaRemoveAmount);

            _leftAnimation.Duration = timeSpan;
            _topAnimation.Duration = timeSpan;
            _opacityAnimation.Duration = timeSpan;
            _opacityAnimation.Completed += (sender, args) => OnAnimationComplete();
            _optionsVersion = OptionPageGeneral.OptionsVersion;
        }

       

        private void OnAnimationComplete()
        {
            _image.Visibility = Visibility.Hidden;
            ParticleCount--;
            _afterExplode(this);
        }
        

        /// <summary>
        /// 爆炸位置
        /// </summary>
        /// <param name="top"></param>
        /// <param name="left"></param>
        public void Explode(double top, double down,double left ,double right)
        {
            
            int position_X = (int) (Random.NextDouble() * (right- left));
            int position_Y = SystemConfig.SnowResolutionHeight;
            
            //如果超過目前最大粒子數量
            if (ParticleCount > SystemConfig.MaxParticleCount)
                return;

            ParticleCount++;

            //如果設定有被更改過
            if (_optionsVersion != OptionPageGeneral.OptionsVersion) InitializeOptions();

            var leftVelocity = Random.NextDouble() * SnowConfig.MaxSideVelocity * Random.NextSignSwap();

            _leftAnimation.From = position_X;
            _leftAnimation.To = position_X - (_iterations * leftVelocity);

            _topAnimation.From = top;
            _topAnimation.To = top + position_Y;

            _image.Visibility = Visibility.Visible;
            _image.BeginAnimation(Canvas.LeftProperty, _leftAnimation);
            _image.BeginAnimation(Canvas.TopProperty, _topAnimation);
            _image.BeginAnimation(Image.OpacityProperty, _opacityAnimation);

            
        }



        /// <summary>
        /// 爆炸位置
        /// </summary>
        /// <param name="top"></param>
        /// <param name="left"></param>
        /*
        public void Explode_OLD(double top, double left)
        {
            top = 0;
            left = 0;

            if (ParticleCount > MaxParticleCount)
                return;
            ParticleCount++;
            if (_optionsVersion != OptionPageGeneral.OptionsVersion) InitializeOptions();
            var upVelocity = Random.NextDouble() * MaxUpVelocity;
            var leftVelocity = Random.NextDouble() * MaxSideVelocity * Random.NextSignSwap();
            _leftAnimation.From = left;
            _leftAnimation.To = left - (_iterations * leftVelocity);
            _topAnimation.From = top;
            _topAnimation.By = -upVelocity;
            _image.Visibility = Visibility.Visible;
            _image.BeginAnimation(Canvas.LeftProperty, _leftAnimation);
            _image.BeginAnimation(Canvas.TopProperty, _topAnimation);
            _image.BeginAnimation(Image.OpacityProperty, _opacityAnimation);

            ParticleCount--;
        }
        */
    }
}