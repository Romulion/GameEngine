using System;
using System.Diagnostics;

namespace Toys
{
    public class Time
    {
        public double RenderTime { get; internal set; }
        public double UpdateTime { get; internal set; }
        public float FrameTime { get; internal set; }
        Stopwatch stopwatch;
        static double resolution = (double)1000 / Stopwatch.Frequency;

        public Time()
        {
            stopwatch = new Stopwatch();
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
