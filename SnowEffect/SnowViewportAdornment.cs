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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnvDTE;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using ShowEffect.Extensions;
using Package = Microsoft.VisualStudio.Shell.Package;
using System.Windows.Forms;
using System.Windows.Media.Animation;
using System.Windows;

namespace ShowEffect
{
    /// <summary>
    /// Adornment class that draws a square box in the top right hand corner of the viewport
    /// </summary>
    internal sealed class SnowViewportAdornment
    {
        [ThreadStatic]
        private static Random _random;

        /// <summary>
        /// The layer for the adornment.
        /// </summary>
        private readonly IAdornmentLayer _adornmentLayer;

        /// <summary>
        /// Text view to add the adornment on.
        /// </summary>
        private readonly IWpfTextView _view;


        private readonly Timer _timer;

        public int ExplosionAmount { get; set; } = 2;

        public int ExplosionDelay { get; set; } = 50;

        public int MaxShakeAmount { get; set; } = 5;

        /// <summary>
        /// Store the last time the user pressed a key. 
        /// To maintain a combo, the user must keep pressing keys at least every x seconds.
        /// </summary>
        public DateTime LastKeyPress { get; set; } = DateTime.Now;

        public int ComboStreak { get; set; }

        private const int MinMsBetweenShakes = 10;

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
        

        /// <summary>
        /// Initializes a new instance of the <see cref="SnowViewportAdornment"/> class.
        /// Creates a square image and attaches an event handler to the layout changed event that
        /// adds the the square in the upper right-hand corner of the TextView via the adornment layer
        /// </summary>
        /// <param name="view">The <see cref="IWpfTextView"/> upon which the adornment will be drawn</param>
        public SnowViewportAdornment(IWpfTextView view)
        {
            if (view == null)
            {
                throw new ArgumentNullException("view");
            }

            //目前設定0.3秒呼叫一次
            _timer = new Timer();
            _timer.Interval = 300;
            _timer.Start();
            _timer.Tick+= Timer_Tickle;

            _view = view;
            _view.TextBuffer.Changed += TextBuffer_Changed;
            _view.TextBuffer.PostChanged += TextBuffer_PostChanged;
            _adornmentLayer = view.GetAdornmentLayer("ExplosionViewportAdornment");
            _explosionParticles =
                new ConcurrentBag<SnowParticle>(
                    Enumerable.Repeat<Func<SnowParticle>>(NewParticle, (int) (TypingConfig.SnowPerPress * 2))
                        .Select(result => result()));

        }

        private async void TypingSnowEffect(TextContentChangedEventArgs e)
        {
            if ((DateTime.Now - LastKeyPress).TotalMilliseconds < MinMsBetweenShakes)
            {
                return;
            }

            if (e.Changes != null && e.Changes.Count > 0)
            {
                try
                {
                    await HandleChange(e.Changes.Sum(x => x.Delta));
                }
                catch
                {
                    //Ignore, not critical that we catch it
                }
            }
        }


        private async void TimerSnowEffect(int intervalTime)
        {
            if ((DateTime.Now - LastKeyPress).TotalMilliseconds < MinMsBetweenShakes)
            {
                return;
            }

            try
            {
               
                //落雪特效
                if (SystemConfig.ParticlesEnabled)
                {
                    int count = TimerConfig.SnowParSecond * intervalTime / 1000;
                    for (uint i = 0; i < count; i++)
                    {
                        GetExplosionParticle().Explode(_view.ViewportTop, _view.ViewportBottom, _view.ViewportLeft, _view.ViewportRight);
                    }
                }
            }
            catch
            {

            }
        }

        private ConcurrentBag<SnowParticle> _explosionParticles;

        private SnowParticle GetExplosionParticle()
        {
            SnowParticle result;
            if (!_explosionParticles.TryTake(out result))
            {
                result = NewParticle();
            }
            return result;
        }

        private SnowParticle NewParticle()
        {
            return new SnowParticle(_adornmentLayer,
                    (DTE)Package.GetGlobalService(typeof(DTE)),
                    particle => _explosionParticles.Add(particle));
        }

        /// <summary>
        /// 處理落雪和搖動特效
        /// </summary>
        /// <param name="delta"></param>
        /// <returns></returns>
        private async Task HandleChange(int delta)
        {
            if (ComboCheck())
            {
                if (SystemConfig.ParticlesEnabled)
                {
                    for (uint i = 0; i < TypingConfig.SnowPerPress; i++)
                    {
                        GetExplosionParticle().Explode(_view.ViewportTop, _view.ViewportBottom, _view.ViewportLeft, _view.ViewportRight);
                    }
                }
                
                if (TypingConfig.ShakeEnabled)
                {
                    await Shake(delta);
                }
            }
        }

        private async Task Shake(int delta)
        {
            for (int i = 0; i < Math.Abs(delta) && i < MaxShakeAmount; i++)
            {
                int leftAmount = ExplosionAmount * Random.NextSignSwap(),
                    topAmount = ExplosionAmount * Random.NextSignSwap();

                _view.ViewportLeft += leftAmount;
                _view.ViewScroller.ScrollViewportVerticallyByPixels(topAmount);
                await Task.Delay(ExplosionDelay);
                _view.ViewportLeft -= leftAmount;
                _view.ViewScroller.ScrollViewportVerticallyByPixels(-topAmount);
            }
        }

        /// <summary>
        /// Keep track of how many keypresses the user has done and returns whether Snow Effect should be activated for each change.
        /// </summary>
        /// <returns>True if Snow Effect should be activated for this change. False if Snow Effect should be ignored for this change.</returns>
        private bool ComboCheck()
        {
            if (TypingConfig.ComboActivationThreshold == 0)
            {
                LastKeyPress = DateTime.Now;
                return true;
            }
            var now = DateTime.Now;
            ComboStreak++;

            if (LastKeyPress.AddMilliseconds(TypingConfig.ComboTimeout) < now) // More than x ms since last key-press. Combo has been broken.
            {
                ComboStreak = 1;
            }

            LastKeyPress = now;
            var activateSnowEffect = ComboStreak >= TypingConfig.ComboActivationThreshold; // Activate SnowEffect if number of keypresses exceeds the threshold for activation
            return activateSnowEffect; // Perhaps different levels for Snow-Effect intensity? First just particles, then screen shake?
        }

        /// <summary>
        /// 如果有文字輸入
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBuffer_Changed(object sender, TextContentChangedEventArgs e)
        {
            TypingSnowEffect(e);
        }

        /// <summary>
        /// 定時器呼叫
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Tickle(object sender, EventArgs e)
        {
            Timer timer = (Timer)sender;
            TimerSnowEffect(timer.Interval);
        }

        private void TextBuffer_PostChanged(object sender, EventArgs e)
        {

        }
    }
}