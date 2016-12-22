/*
The MIT License(MIT)

Copyright(c) 2015 Liam Morrow

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
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows.Media;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.Settings;
using Microsoft.Win32;
using System.Drawing;

namespace ShowEffect
{
    /// <summary>
    /// 設定選項
    /// </summary>
    public class OptionPageGeneral : DialogPage
    {
        /// <summary>
        /// use this to force reloading of settings
        /// each tiem a setting change shere we increment the number
        /// each explosion particle checks it's version of options against
        /// this one to check for updates
        /// </summary>
        public static uint OptionsVersion = 0;

        //=========================CATEGORY_SNOW===========================
        const string CATEGORY_SYSTEM = "Main Setting";

        /// <summary>
        /// 要不要有粒子特效
        /// </summary>
        [Category(CATEGORY_SYSTEM)]
        [DisplayName("[Effect]  Particles enabled")]
        [Description("Sets the particles are enabled or not.")]
        public bool ParticlesEnabled
        {
            get { return SystemConfig.ParticlesEnabled; }
            set
            {
                SystemConfig.ParticlesEnabled = value;
                OptionsVersion++;
            }
        }

        /// <summary>
        /// 最大粒子數量
        /// 50~5000
        /// </summary>
        [Category(CATEGORY_SYSTEM)]
        [DisplayName("[Performance]  Max particle count")]
        [Description("The maximum amount of particles at one time.")]
        public int MaxParticleCount
        {
            get { return SystemConfig.MaxParticleCount; }
            set
            {
                if (value < 50)
                    MaxParticleCount = 50;
                else if (value > 5000)
                    MaxParticleCount = 5000;
                else
                    SystemConfig.MaxParticleCount = value;

                OptionsVersion++;
            }
        }

        /// <summary>
        /// FPS
        /// 5~240
        /// </summary>
        [Category(CATEGORY_SYSTEM)]
        [DisplayName("[Performance]  FPS (5~240)")]
        [Description("Adjust the display FPS, the setting will be used until next open.")]
        public int FPS
        {
            get { return SystemConfig.FPS; }
            set
            {
                if (value < 5)
                    FPS = 5;
                else if (value > 240)
                    FPS = 240;
                else
                    SystemConfig.FPS = value;

                //OptionsVersion++;
            }
        }

        /// <summary>
        /// 參考下雪高度
        /// 要是用百分比還是解析度算
        /// </summary>
        [Category(CATEGORY_SYSTEM)]
        [DisplayName("[Display]  Snow display height (Precentage or Pixel)")]
        [Description("Snow display height.(Precentage or Pixel)")]
        public SystemConfig.SnowFallDownHeightMode SnowFallDownHeightMode
        {
            get { return SystemConfig.FallDownHeightMode; }
            set
            {
                SystemConfig.FallDownHeightMode = value;

                OptionsVersion++;
            }
        }

        /// <summary>
        /// 顯示高度(解析度)
        /// 10~3000
        /// </summary>
        [Category(CATEGORY_SYSTEM)]
        [DisplayName("[Display]  Snow display height pixel (10~3000 pixel)")]
        [Description("Snow display height pixel.(10~3000 pixel)")]
        public int SnowResolutionHeight
        {
            get { return SystemConfig.SnowResolutionHeight; }
            set
            {
                if (value < 10)
                    SnowResolutionHeight = 10;
                else if (value > 3000)
                    SnowResolutionHeight = 3000;
                else
                    SystemConfig.SnowResolutionHeight = value;
                OptionsVersion++;
            }
        }

        /// <summary>
        /// 設定目前對應解析度
        /// 10~100
        /// </summary>
        [Category(CATEGORY_SYSTEM)]
        [DisplayName("[Display]  Snow Display height precentage (10~100%)")]
        [Description("Snow Display height precentage.(10~100%)")]
        public int SnowResolutionHeightPrecentage
        {
            get { return (int)(SystemConfig.SnowResolutionHeightPrecentage * 100); }
            set
            {
                if (value < 10)
                    SnowResolutionHeightPrecentage = 10;
                else if (value > 100)
                    SnowResolutionHeightPrecentage = 100;
                else
                    SystemConfig.SnowResolutionHeightPrecentage = (float)value/100;
                OptionsVersion++;
            }
        }


        //=========================CATEGORY_SNOW===========================
        const string CATEGORY_SNOW = "Snow";

        /// <summary>
        /// 設定噴發顏色
        /// </summary>
        [Category(CATEGORY_SNOW)]
        [DisplayName("[Color]  Snow color")]
        [Description("The color of snow.")]
        public System.Windows.Media.Color Color
        {
            get { return SnowConfig.Color; }
            set
            {
                SnowConfig.Color = value;
                OptionsVersion++;
            }
        }

        /// <summary>
        /// 設定目前粒子是不是要照文字顏色
        /// </summary>
        [Category(CATEGORY_SNOW)]
        [DisplayName("[Color]  Get color from text (0~100%)")]
        [Description("Get The precentage of refrence color from text.")]
        public float MixGetColorFromEnvironment
        {
            get { return SnowConfig.MixGetColorFromEnvironment*100; }
            set
            {
                if (value < 0)
                    MixGetColorFromEnvironment = 0;
                else if (value > 100)
                    MixGetColorFromEnvironment = 100;
                else
                    SnowConfig.MixGetColorFromEnvironment = value/100;

                OptionsVersion++;
            }
        }

        /// <summary>
        /// 要不要隨機，影響程度
        /// </summary>
        [Category(CATEGORY_SNOW)]
        [DisplayName("[Color]  Explosion particle randomized color (0~100%)")]
        [Description("Get The precentage of refrence color from random.")]
        public float MixRandomColor
        {
            get { return SnowConfig.MixRandomColor*100; }
            set
            {
                if (value < 0)
                    MixRandomColor = 0;
                else if (value > 100)
                    MixRandomColor = 100;
                else
                    SnowConfig.MixRandomColor = value/100;
                OptionsVersion++;
            }
        }

        /// <summary>
        /// 設定噴發圖片
        /// </summary>
        [Category(CATEGORY_SNOW)]
        [DisplayName("[Image]  Explosion particle image (from file)")]
        [Description("The image of falling down snow.")]
        public Image image
        {
            
            get
            {
                if (SnowConfig.Image == null)
                {
                    Image returnImage = new Bitmap(10,10);
                }
                    
                return SnowConfig.Image;
            }
            set
            {
                SnowConfig.Image = value;
                OptionsVersion++;
            }
        }

        /// <summary>
        /// 雪的大小
        /// </summary>
        [Category(CATEGORY_SNOW)]
        [Description("[Size]  Snow size (0.1~1000)")]
        [DisplayName("Snow size : )")]
        public float SnowSize
        {
            get { return SnowConfig.SnowSize; }
            set
            {
                if (value < 0.1f)
                    SnowSize = 0.1f;
                else if (value > 1000)
                    SnowSize = 1000;
                else
                    SnowConfig.SnowSize = value;
                OptionsVersion++;
            }
        }


        /// <summary>
        /// 最大偏移量
        /// 0~500
        /// </summary>
        [Category(CATEGORY_SNOW)]
        [DisplayName("[Snow Effect]  Max side velocity(0~500)")]
        [Description("The maximum sideward velocity of the particles.")]
        public double MaxSideVelocity
        {
            get { return SnowConfig.MaxSideVelocity; }
            set
            {
                if (value < 0)
                    MaxSideVelocity = 0;
                else if (value > 500)
                    MaxSideVelocity = 500;
                else
                    SnowConfig.MaxSideVelocity = value;
                OptionsVersion++;
            }
        }

        /// <summary>
        /// 偏移量
        /// 0~1
        /// </summary>
        [Category(CATEGORY_SNOW)]
        [DisplayName("[Snow Effect]  Amplitude (0~1)")]
        [Description("The strength of the Amplitude.")]
        public double Amplitude
        {
            get { return SnowConfig.Amplitude; }
            set
            {
                if (value < 0)
                    Amplitude = 0;
                else if (value > 1)
                    Amplitude = 1;
                else
                    SnowConfig.Amplitude = value;

                OptionsVersion++;
            }
        }

        /// <summary>
        /// 開始的透明度
        /// 10~100%
        /// </summary>
        [Category(CATEGORY_SNOW)]
        [DisplayName("[Snow Effect]  Start alpha (10~100%)")]
        [Description("The starting opacity of the particle.")]
        public double StartAlpha
        {
            get { return SnowConfig.StartAlpha*100; }
            set
            {
                
                if (value < 10)
                    StartAlpha = 10;
                else if (value > 100)
                    StartAlpha = 100;
                else
                    SnowConfig.StartAlpha = value/100;

                OptionsVersion++;
            }
        }

        /// <summary>
        /// 落下速度(pixel/秒)
        /// </summary>
        [Category(CATEGORY_SNOW)]
        [DisplayName("[Snow Effect]  Snow fall down speed (30~500(pixel/second))")]
        [Description("Snow fall down speed.")]
        public double FallDownSpeed
        {
            get { return SnowConfig.FallDownSpeed; }
            set
            {
                if (value < 30)
                    FallDownSpeed = 30;
                else if (value > 500)
                    FallDownSpeed = 500;
                else
                    SnowConfig.FallDownSpeed = value;

                OptionsVersion++;
            }
        }
        

        //=========================CATEGORY_Time===========================
        const string CATEGORY_Time = "Time";


        /// <summary>
        /// 基礎顏色
        /// </summary>
        [Category(CATEGORY_Time)]
        [DisplayName("[Number]  Snow number par second (1~300)")]
        [Description("The number of snow par second.")]
        public int SnowParSecond
        {
            get { return TimerConfig.SnowParSecond; }
            set
            {
                if (value < 1)
                    SnowParSecond = 1;
                else if (value > 300)
                    SnowParSecond = 300;
                else
                    TimerConfig.SnowParSecond = value;

                OptionsVersion++;
            }
        }


        /// <summary>
        /// 飄雪時間
        /// 經過多久沒有打字就會停止飄雪
        /// 或是 -1 = 無限大
        /// -1~3600秒
        /// </summary>
        [Category(CATEGORY_Time)]
        [DisplayName("[Time]  Time snow stop fall when no typing (-1(infinite)~3600(s))")]
        [Description("Snow start time after typing.")]
        public int SnowTimeAfterTyping
        {
            get { return TimerConfig.SnowTimeAfterTyping; }
            set
            {
                if (value < -1)
                    SnowTimeAfterTyping = -1;
                else if (value > 3600)
                    SnowTimeAfterTyping = 3600;
                else
                    TimerConfig.SnowTimeAfterTyping = value;
            }
        }

        /// <summary>
        /// 打字過後經過多久才會開始飄雪
        /// 0~無限大
        /// </summary>
        [Category(CATEGORY_Time)]
        [DisplayName("[Time]  Time snow start fall after typing (>=0)")]
        [Description("Time snow start fall after typing.")]
        public int SnowWaitingTimeAfteerTyping
        {
            get { return TimerConfig.SnowWaitingTimeAfteerTyping; }
            set
            {
                if (value < 0)
                    SnowWaitingTimeAfteerTyping = 0;
                else
                TimerConfig.SnowWaitingTimeAfteerTyping = value;
            }
        }

        //=========================CATEGORY_Typing===========================
        const string CATEGORY_Typing = "Typing";

        /// <summary>
        /// 按一下會噴出多少雪
        /// 0~100，如果是0就自動把噴雪關掉
        /// </summary>
        [Category(CATEGORY_Typing)]
        [Description("[Number]  Number of snow per press (0~100)")]
        [DisplayName("Number of snow Per Press.")]
        public int SnowPerPress
        {
            get { return TypingConfig.SnowPerPress; }
            set
            {
                if (value < 0)
                    SnowPerPress = 0;
                else if (value > 100)
                    SnowPerPress = 100;
                else
                    TypingConfig.SnowPerPress = value;

                OptionsVersion++;
            }
        }

        /// <summary>
        /// 按一下會 額外 噴出多少雪
        /// 0~50
        /// </summary>
        [Category(CATEGORY_Typing)]
        [Description("[Number]  Addition snow per combo press (0~50)")]
        [DisplayName("Addition snow per combo press.")]
        public int ComboAdditionSnowNumber
        {
            get { return TypingConfig.ComboAdditionSnowNumber; }
            set
            {
                if (value < 0)
                    ComboAdditionSnowNumber = 0;
                else if (value > 50)
                    ComboAdditionSnowNumber = 50;
                else
                    TypingConfig.ComboAdditionSnowNumber = value;

                OptionsVersion++;
            }
        }

        /// <summary>
        /// 延遲時間
        /// </summary>
        [Category(CATEGORY_Typing)]
        [Description("[Snow]  Delay show time after typing (milliseconds)")]
        [DisplayName("Delay show time after typing.")]
        public int FrameDelay
        {
            get { return TypingConfig.SnowFallDelay; }
            set
            {
                if (value < 0)
                    FrameDelay = 0;
                else if (value > 50)
                    FrameDelay = 50;
                else
                    TypingConfig.SnowFallDelay = value;

                OptionsVersion++;
            }
        }

        /// <summary>
        /// 陸續噴完雪所需要的時間
        /// 0~10000ms
        /// </summary>
        [Category(CATEGORY_Typing)]
        [Description("[Snow]  The time typing snow all fall (1~10000ms)")]
        [DisplayName("The time typing snow all fall.")]
        public int SnowFallRangeTime
        {
            get { return TypingConfig.SnowFallRangeTime; }
            set
            {
                if (value < 0)
                    SnowFallRangeTime = 0;
                else if (value > 10000)
                    SnowFallRangeTime = 10000;
                else
                    TypingConfig.SnowFallRangeTime = value;

                OptionsVersion++;
            }
        }

        /// <summary>
        /// 打字時螢幕要不要搖動
        /// </summary>
        [Category(CATEGORY_Typing)]
        [DisplayName("[Shake]  Screen shake when typing")]
        [Description("Sets whether the screen shakes")]
        public bool ShakeEnabled
        {
            get { return TypingConfig.ShakeEnabled; }
            set
            {
                TypingConfig.ShakeEnabled = value;
                OptionsVersion++;
            }
        }

        /// <summary>
        /// 要打幾個字後才會開始噴雪
        /// 0~1000
        /// </summary>
        [Category(CATEGORY_Typing)]
        [DisplayName("[Combo]  Combo threshold (0~1000words)")]
        [Description("The number of keypresses required to turn on Snow Effect. Set to 0 to always enable Snow Effect.")]
        public int ComboThreshold
        {
            get { return TypingConfig.ComboActivationThreshold; }
            set
            {
                if (value < 0)
                    ComboThreshold = 0;
                else if (value > 1000)
                    ComboThreshold = 1000;
                else
                    TypingConfig.ComboActivationThreshold = value;

                OptionsVersion++;
            }
        }

        /// <summary>
        /// 經過多久後combo 會斷掉
        /// 0~100000(100秒)
        /// </summary>
        [Category(CATEGORY_Typing)]
        [DisplayName("[Combo]  Combo timeout (0~100000ms)")]
        [Description("Combo timeout")]
        public int ComboTimeout
        {
            get { return TypingConfig.ComboTimeout; }
            set
            {
                if (value < 0)
                    ComboThreshold = 0;
                else if (value > 100000)
                    ComboThreshold = 100000;
                else
                    TypingConfig.ComboTimeout = value;

                OptionsVersion++;
            }
        }
    }
}