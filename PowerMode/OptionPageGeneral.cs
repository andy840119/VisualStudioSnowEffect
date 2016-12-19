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

namespace PowerMode
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
        const string CATEGORY_SYSTEM = "System Setting";

        /// <summary>
        /// 要不要有粒子特效
        /// </summary>
        [Category(CATEGORY_SYSTEM)]
        [DisplayName("Particles Enabled")]
        [Description("Sets whether the particles are enabled")]
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
        /// 設定目前對應解析度
        /// </summary>
        [Category(CATEGORY_SYSTEM)]
        [DisplayName("Snow Display width")]
        [Description("Recommand set as your Monitor's width , For example : 1920")]
        public int SnowResolutionHeight
        {
            get { return SystemConfig.SnowResolutionHeight; }
            set
            {
                SystemConfig.SnowResolutionHeight = value;
                OptionsVersion++;
            }
        }

        /// <summary>
        /// 最大粒子數量
        /// </summary>
        [Category(CATEGORY_SYSTEM)]
        [DisplayName("Max Particle Count")]
        [Description("The maximum amount of particles at one time")]
        public int MaxParticleCount
        {
            get { return SystemConfig.MaxParticleCount; }
            set
            {
                SystemConfig.MaxParticleCount = value;
                OptionsVersion++;
            }
        }


        //=========================CATEGORY_SNOW===========================
        const string CATEGORY_SNOW = "Snow";

        /// <summary>
        /// 設定噴發顏色
        /// </summary>
        [Category(CATEGORY_SNOW)]
        [DisplayName("[Color]  Explosion Particle Color")]
        [Description("The color of the explosion particle")]
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
        [DisplayName("[Color]  Get color from Text (0~100)")]
        [Description("Get The Precentage of Text Color to snow")]
        public float MixGetColorFromEnvironment
        {
            get { return SnowConfig.MixGetColorFromEnvironment; }
            set
            {
                SnowConfig.MixGetColorFromEnvironment = value;
                OptionsVersion++;
            }
        }

        /// <summary>
        /// 要不要隨機，影響程度
        /// </summary>
        [Category(CATEGORY_SNOW)]
        [DisplayName("[Color]  Explosion Particle Randomized Color")]
        [Description("Whether to use a random color. Overrides Explosion Particle Color if set.")]
        public float MixRandomColor
        {
            get { return SnowConfig.MixRandomColor; }
            set
            {
                SnowConfig.MixRandomColor = value;
                OptionsVersion++;
            }
        }

        /// <summary>
        /// 雪的大小
        /// </summary>
        [Category(CATEGORY_SNOW)]
        [Description("[Size]  Snow size")]
        [DisplayName("Snow size : )")]
        public double SnowSize
        {
            get { return SnowConfig.SnowSize; }
            set
            {
                SnowConfig.SnowSize = value;
                OptionsVersion++;
            }
        }


        /// <summary>
        /// 最大偏移量
        /// </summary>
        [Category(CATEGORY_SNOW)]
        [DisplayName("[Snow Effect]  Max Side Velocity")]
        [Description("The maximum sideward velocity of the particles")]
        public double MaxSideVelocity
        {
            get { return SnowConfig.MaxSideVelocity; }
            set
            {
                SnowConfig.MaxSideVelocity = value;
                OptionsVersion++;
            }
        }

        /// <summary>
        /// 重力
        /// </summary>
        [Category(CATEGORY_SNOW)]
        [DisplayName("[Snow Effect]  Gravity")]
        [Description("The strength of the gravity")]
        public double Gravity
        {
            get { return SnowConfig.Gravity; }
            set
            {
                SnowConfig.Gravity = value;
                OptionsVersion++;
            }
        }

        /// <summary>
        /// 開始的透明度
        /// </summary>
        [Category(CATEGORY_SNOW)]
        [DisplayName("[Snow Effect]  Start Alpha")]
        [Description("The starting opacity of the particle. Affects lifetime.")]
        public double StartAlpha
        {
            get { return SnowConfig.StartAlpha; }
            set
            {
                SnowConfig.StartAlpha = value;
                OptionsVersion++;
            }
        }

        /// <summary>
        /// 設定透明度減少量
        /// </summary>
        [Category(CATEGORY_SNOW)]
        [DisplayName("[Snow Effect]  Alpha Decrement Amount")]
        [Description("The amount of alpha removed every frame.")]
        public double AlphaRemoveAmount
        {
            get { return SnowConfig.AlphaRemoveAmount; }
            set
            {
                SnowConfig.AlphaRemoveAmount = value;
                OptionsVersion++;
            }
        }
        

        //=========================CATEGORY_Time===========================
        const string CATEGORY_Time = "Time";


        /// <summary>
        /// 基礎顏色
        /// </summary>
        [Category(CATEGORY_Time)]
        [DisplayName("[Number]  Show Par Second")]
        [Description("The number of snow par Second.")]
        public int SnowParSecond
        {
            get { return TimerConfig.SnowParSecond; }
            set { TimerConfig.SnowParSecond = value; }
        }

        //=========================CATEGORY_Typing===========================
        const string CATEGORY_Typing = "Typing";

        /// <summary>
        /// 按一下會噴出多少雪
        /// </summary>
        [Category(CATEGORY_Typing)]
        [Description("[Number]  Snow Per Press")]
        [DisplayName("Snow Per Press")]
        public int SnowPerPress
        {
            get { return TypingConfig.SnowPerPress; }
            set
            {
                TypingConfig.SnowPerPress = value;
                OptionsVersion++;
            }
        }

        /// <summary>
        /// 按一下會 額外 噴出多少雪
        /// </summary>
        [Category(CATEGORY_Typing)]
        [Description("[Number]  Addition Snow Per combo Press")]
        [DisplayName("Addition Snow Per combo Press")]
        public int ComboAdditionSnowNumber
        {
            get { return TypingConfig.ComboAdditionSnowNumber; }
            set
            {
                TypingConfig.ComboAdditionSnowNumber = value;
                OptionsVersion++;
            }
        }

        /// <summary>
        /// 延遲時間
        /// </summary>
        [Category(CATEGORY_Typing)]
        [Description("[Snow]  Delay between Frames (milliseconds)")]
        [DisplayName("Frame Delay")]
        public int FrameDelay
        {
            get { return TypingConfig.SnowFallDelay; }
            set
            {
                TypingConfig.SnowFallDelay = value;
                OptionsVersion++;
            }
        }

        /// <summary>
        /// 陸續噴完雪所需要的時間
        /// </summary>
        [Category(CATEGORY_Typing)]
        [Description("[Snow]  The time all snow fall")]
        [DisplayName("The time all snow fall")]
        public int SnowFallRangeTime
        {
            get { return TypingConfig.SnowFallRangeTime; }
            set
            {
                TypingConfig.SnowFallRangeTime = value;
                OptionsVersion++;
            }
        }

        /// <summary>
        /// 打字時螢幕要不要搖動
        /// </summary>
        [Category(CATEGORY_Typing)]
        [DisplayName("[Shake]  Screen Shake")]
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
        /// </summary>
        [Category(CATEGORY_Typing)]
        [DisplayName("[Combo]  Combo threshold")]
        [Description("The number of keypresses required to turn on Power Mode. Set to 0 to always enable Power Mode.")]
        public int ComboThreshold
        {
            get { return TypingConfig.ComboActivationThreshold; }
            set
            {
                TypingConfig.ComboActivationThreshold = value;
                OptionsVersion++;
            }
        }

        /// <summary>
        /// 經過多久後combo 會斷掉
        /// </summary>
        [Category(CATEGORY_Typing)]
        [DisplayName("[Combo]  Combo timeout")]
        [Description("Combo timeout")]
        public int ComboTimeout
        {
            get { return TypingConfig.ComboTimeout; }
            set
            {
                TypingConfig.ComboTimeout = value;
                OptionsVersion++;
            }
        }
    }
}