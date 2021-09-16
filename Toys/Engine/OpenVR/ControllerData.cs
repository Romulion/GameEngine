using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Mathematics;

namespace Toys.VR
{
    class ControllerData
    {
        //Fields to be initialzed by iterateAssignIds() and setHands()
        public int deviceId;  // Device ID according to the SteamVR system
        public int hand;       // 0=invalid 1=left 2=right
        public int idtrigger;  // Trigger axis id
        public int idpad;      // Touchpad axis id

        //Analog button data to be set in ContollerCoods()
        public float padX;
        public float padY;
        public float trigVal;

        //Position set in ControllerCoords()
        public Vector3 pos;
        public Quaternion rot;

        public bool isValid;

        public string RenderModelName;

        public ControllerData()
        {
            deviceId = -1;
            hand = -1;
            idtrigger = -1;
            idpad = -1;
        }
    };
}
