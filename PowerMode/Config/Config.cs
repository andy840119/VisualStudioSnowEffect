using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace PowerMode
{
    /// <summary>
    /// 初始化整體設定
    /// </summary>
    public class Config
    {
        static protected OptionPageGeneral page;
        static Config()
        {
           
        }
    }


    /// <summary>
    /// 有關系統的設定，例如解析度等等
    /// </summary>
    public class SystemConfig : Config
    {
        static SystemConfig()
        {
            var service = ServiceProvider.GlobalProvider.GetService(typeof(SPowerMode)) as IPowerMode;
            if (service == null) return;
            page = service.Package.General;

            ParticlesEnabled = page.ParticlesEnabled;
            SnowResolutionHeight = page.SnowResolutionHeight;
            MaxParticleCount = page.MaxParticleCount;
        }
        /// <summary>
        /// 要不要有下雪特效
        /// 如果關掉就是兩邊都關掉了
        /// </summary>
        public static bool ParticlesEnabled { get; set; } = true;

        /// <summary>
        /// 設定雪落下的pixel
        /// </summary>
        public static int SnowResolutionHeight { get; set; } = 1000;

        /// <summary>
        /// 最多有多少雪
        /// </summary>
        public static int MaxParticleCount { get; set; } = 8787;// int.MaxValue;
    }

    /// <summary>
    /// 有關雪的設定
    /// 大小顆，等等之類的
    /// </summary>
    public class SnowConfig : Config
    {
        static SnowConfig()
        {
            var service = ServiceProvider.GlobalProvider.GetService(typeof(SPowerMode)) as IPowerMode;
            if (service == null) return;
            page = service.Package.General;

            //參數設定
            Color = page.Color;
            MixGetColorFromEnvironment = page.MixGetColorFromEnvironment;
            MixRandomColor = page.MixRandomColor;
            SnowSize = page.SnowSize;
            MaxSideVelocity = page.MaxSideVelocity;
            Gravity = page.Gravity;
            StartAlpha = page.StartAlpha;
            AlphaRemoveAmount = page.AlphaRemoveAmount;

        }
        /// <summary>
        /// 基底顏色
        /// </summary>
        public static Color Color { get; set; } = Colors.Black;

        /// <summary>
        /// 從系統上取得目前顏色
        /// 0~1
        /// </summary>
        public static float MixGetColorFromEnvironment { get; set; } = 0;

        /// <summary>
        /// 要不要隨機顏色
        /// 0~1
        /// </summary>
        public static float MixRandomColor { get; set; } = 0;

        /// <summary>
        /// 雪的顆粒大小
        /// </summary>
        public static double SnowSize { get; set; } = 10;

        /// <summary>
        /// 左右噴的速度
        /// </summary>
        public static double MaxSideVelocity { get; set; } = 2;

        /// <summary>
        /// 重力
        /// </summary>
        public static double Gravity { get; set; } = 0.3;


        /// <summary>
        /// 開始透明度
        /// </summary>
        public static double StartAlpha { get; set; } = 0.9;

        /// <summary>
        /// 透明度遞減
        /// </summary>
        public static double AlphaRemoveAmount { get; set; } = 0.045;


    }

    /// <summary>
    /// 設定時間
    /// </summary>
    class TimerConfig : Config
    {
        static TimerConfig()
        {
            var service = ServiceProvider.GlobalProvider.GetService(typeof(SPowerMode)) as IPowerMode;
            if (service == null) return;
            page = service.Package.General;

            SnowParSecond = page.SnowParSecond;
        }

        /// <summary>
        /// 每秒鐘落下幾顆雪
        /// </summary>
        public static int SnowParSecond { get; set; } = 15;
    }

    /// <summary>
    /// 設定打字時的行為
    /// </summary>
    class TypingConfig : Config
    {
        static TypingConfig()
        {
            var service = ServiceProvider.GlobalProvider.GetService(typeof(SPowerMode)) as IPowerMode;
            if (service == null) return;
            page = service.Package.General;

            SnowPerPress = page.SnowPerPress;
            ComboAdditionSnowNumber = page.ComboAdditionSnowNumber;
            SnowFallDelay = page.FrameDelay;
            SnowFallRangeTime = page.SnowFallRangeTime;
            ShakeEnabled = page.ShakeEnabled;
            ComboActivationThreshold = page.ComboThreshold;
            ComboTimeout = page.ComboTimeout;
        }

        /// <summary>
        /// 每按一下鍵盤噴幾顆雪
        /// </summary>
        public static int SnowPerPress { get; set; } = 10;

        /// <summary>
        /// 在combo內如果多按一下就平均增加幾顆雪
        /// </summary>
        public static int ComboAdditionSnowNumber { get; set; } = 3;

        /// <summary>
        /// 延遲時間
        /// </summary>
        public static int SnowFallDelay { get; set; } = 17;

        /// <summary>
        /// 間隔多久後會把目前囤積的雪噴完
        /// </summary>
        public static int SnowFallRangeTime { get; set; } = 200;

        /// <summary>
        /// 打字時要不要有晃動特效
        /// </summary>
        public static bool ShakeEnabled { get; set; } = false;

        /// <summary>
        /// 要大於多少combo後才會開始噴雪
        /// </summary>
        public static int ComboActivationThreshold { get; set; } = 0;

        /// <summary>
        /// 幾秒鐘後combo 斷掉
        /// </summary>
        public static int ComboTimeout { get; set; } = 5000; // In milliseconds

        

        
    }
}
