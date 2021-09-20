﻿using System;
using System.Diagnostics;

namespace Toys
{
    public class Time
    {
        public double RenderTime { get; internal set; }
        public double UpdateTime { get; internal set; }
        public float FrameTime { get; internal set; }
        Stopwatch stopwatch;
        Stopwatch programStarted;
        static double resolution = (double)1000 / Stopwatch.Frequency;
        public long FrameCount { get; internal set; }

        /// <summary>
        /// Time from Program start in ms
        /// </summary>
        public double TimeFromStart { get { return resolution * programStarted.ElapsedTicks; } }

        public Time()
        {
            FrameCount = 0;
            stopwatch = new Stopwatch();
            programStarted = new Stopwatch();
            programStarted.Start();
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
