using System;
using System.Diagnostics;

namespace Toys
{
    public class Time
    {
        public double renderTime { get; internal set; }
        public double updagteTime { get; internal set; }
        Stopwatch stopwatch;
        static double resolution;

        internal Time()
        {
            stopwatch = new Stopwatch();
        }

        static Time()
        {
            resolution = (double)1000 / Stopwatch.Frequency;
        }

        public void Start()
        {
            stopwatch.Start();
        }

        public double Stop()
        {
            double elapsed;
            stopwatch.Stop();
            elapsed = resolution * stopwatch.ElapsedTicks;
            stopwatch.Reset();

            return elapsed;
        }
    }
}
