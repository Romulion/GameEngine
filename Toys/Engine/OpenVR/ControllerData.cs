using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Mathematics;

namespace Toys.VR
{
    public enum ControllerRole{ Left, Right };
    public class ControllerData
    {

        //Analog button data to be set in ContollerCoods()
        public Vector2 stick;
        public bool button1;
        public bool button2;
        public bool trigPress;
        public bool stickPress;
        public bool grabress;
        public float trigVal;
        public float grabVal;


        //Position set in ControllerCoords()
        public Vector3 pos;
        public Quaternion rot;

        public bool isValid;

        public string RenderModelName;

        public ControllerRole controllerRole;

        internal ulong sourceId = 0;
        internal ulong triggerPressId = 0;
        internal ulong stickerPosId = 0;
        internal ulong stickerPressId = 0;
        internal ulong button1PressId = 0;
        internal ulong button2PressId = 0;
        internal ulong grabPressId = 0;
        internal ulong poseId = 0;


        public ControllerData()
        {

        }
    };
}
