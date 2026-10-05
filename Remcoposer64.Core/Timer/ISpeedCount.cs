using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Remcoposer64.Core.Timer
{
    //↓参考にしたもの：
    //https://tocsworld.wordpress.com/2014/03/03/cでの処理時間計測いろいろ/

    public interface ISpeedCount
    {
        void Start();
        double ElapsedSec();
    }

    public class TimeCountByStopWatch : ISpeedCount
    {
        private Stopwatch stopWatch;
        public void Start()
        {
            stopWatch = Stopwatch.StartNew();
        }

        public double ElapsedSec()
        {
            //stopWatch.Stop();
            return (double)stopWatch.ElapsedTicks / Stopwatch.Frequency;
        }

    }

    public class TimeCountByDateTime : ISpeedCount
    {
        private DateTime startDate;
        public void Start()
        {
            startDate = DateTime.Now;
        }

        public double ElapsedSec()
        {
            DateTime endDate = DateTime.Now;
            TimeSpan diff = endDate - startDate;
            return diff.Duration().Ticks / 10000000.0;
        }

    }

    public class TimeCountByPerformanceCounter : ISpeedCount
    {
        [DllImport("kernel32.dll")]
        static extern bool QueryPerformanceCounter(ref long lpPerformanceCount);
        [DllImport("kernel32.dll")]
        static extern bool QueryPerformanceFrequency(ref long lpFrequency);

        private long startCounter;
        public void Start()
        {
            QueryPerformanceCounter(ref startCounter);
        }

        public double ElapsedSec()
        {
            long stopCounter = 0;
            QueryPerformanceCounter(ref stopCounter);
            long frequency = 0;
            QueryPerformanceFrequency(ref frequency);
            return (double)(stopCounter - startCounter) / frequency;
        }
    }
}