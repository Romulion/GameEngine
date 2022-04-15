using System;
using System.Collections.Generic;
using System.Text;

namespace Toys
{
    public class KeyFrameTrigger
    {
        public float FrameId;
        public Action Trigger;

        public KeyFrameTrigger()
        {

        }
        public KeyFrameTrigger(float frameId, Action trigger)
        {
            FrameId = frameId;
            Trigger = trigger;
        }
    }
}
