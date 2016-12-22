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
using ShowEffect.Extensions;

namespace ShowEffect
{
    public class SnowParticle
    {
       

        [ThreadStatic]
        private static Random _random;

        private readonly DTE _service;
        private readonly Action<SnowParticle> _afterExplode;
        private readonly IAdornmentLayer adornmentLayer;

        private Image _image;

        //往左邊的動畫
        private DoubleAnimation _leftAnimation;
        //往上飄移的動畫
        private DoubleAnimation _topAnimation;

        private DoubleAnimation _opacityAnimation;
        private double _animationSecond;
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

        public SnowParticle(IAdornmentLayer adornment, DTE service, Action<SnowParticle> afterExplode)
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
                brushColor = ShowExtension.MixColor(brushColor , (svc.GetThemedWPFColor(EnvironmentColors.PanelTextColorKey)) ,(1- SnowConfig.MixGetColorFromEnvironment), SnowConfig.MixGetColorFromEnvironment);
            }
            else if (SnowConfig.MixRandomColor>0)
            {
                brushColor = ShowExtension.MixColor(brushColor, (Random.NextColor()), (1 - SnowConfig.MixRandomColor), SnowConfig.MixRandomColor);
            }
            else
            {
                brushColor = SnowConfig.Color;
            }

            //設定雪的顯示，圖片或是圓點
            _image = ShowExtension.GetImage(brushColor, SnowConfig.SnowSize);

            // Add the image to the adornment layer and make it relative to the viewport
            adornmentLayer.AddAdornment(AdornmentPositioningBehavior.ViewportRelative,
                null,
                null,
                _image,
                null);

            //初始化
            _leftAnimation = new DoubleAnimation();
            _topAnimation = new DoubleAnimation();
            _opacityAnimation = new DoubleAnimation();

            //設定震幅(原先是)0.0005*37=0.0175
            _topAnimation.EasingFunction = new BackEase {Amplitude = SnowConfig.Amplitude};

            //完成事件
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
            //如果超過目前最大粒子數量
            if (ParticleCount > SystemConfig.MaxParticleCount)
                return;

            //粒子確定會生產
            ParticleCount++;

            //如果設定有被更改過
            if (_optionsVersion != OptionPageGeneral.OptionsVersion)
                InitializeOptions();

            //X的落下位置
            int position_X = (int)(Random.NextDouble() * (right - left));

            //取得飄動偏移量
            var leftVelocity = Random.NextDouble() * SnowConfig.MaxSideVelocity * Random.NextSignSwap();

            //左邊動畫
            _leftAnimation.From = position_X;
            _leftAnimation.To = position_X - (_animationSecond * leftVelocity);
            //上面動畫
            _topAnimation.From = top;
            _topAnimation.To = top + ShowExtension.FallDownHeight(top, down);
            //透明動畫
            _opacityAnimation.From = SnowConfig.StartAlpha;
            _opacityAnimation.To = 0;

            //移動動畫時間(秒)
            _animationSecond = ShowExtension.AnimationTimeByFallDownTime(top, down);
            //算出消失 HH:MM:SS
            var fallDownTimeSpan = TimeSpan.FromMilliseconds(_animationSecond * 1000);
            //動畫時間
            _leftAnimation.Duration = fallDownTimeSpan;
            _topAnimation.Duration = fallDownTimeSpan;
            _opacityAnimation.Duration = fallDownTimeSpan;
            
            //設定到圖片上面
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