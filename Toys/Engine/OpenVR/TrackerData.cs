using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Mathematics;

namespace Toys.VR
{
    class TrackerData
    {

        public int deviceId = -1;   // Device ID according to the SteamVR system
        public Vector3 position;
        public Quaternion rotation;
        public bool isValid;
    }
}
