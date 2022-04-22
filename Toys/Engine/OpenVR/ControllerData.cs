using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Mathematics;

namespace Toys.VR
{
    public enum ControllerButton
    {
        Button1,
        Button2,
        Trigger,
        Stick,
        Grab,
    }
    public enum ControllerRole{ Left, Right };
    public class ControllerData
    {
        internal bool[] triggerPrevState = new bool[5];
        internal bool[] triggerState = new bool[5];
        //Analog button data to be set in ContollerCoods()
        public Vector2 stick;

        internal void SetBoolValue(ControllerButton key, bool value)
        {
            triggerState[(int)key] = value;
        }

        public bool IsChanged(ControllerButton key)
        {
            return triggerPrevState[(int)key] != triggerState[(int)key];
        }

        public bool IsButtonPressed(ControllerButton key)
        {
            return IsChanged(key) && triggerState[(int)key];
        }

        public bool IsButtonReleased(ControllerButton key)
        {
            return IsChanged(key) && !triggerState[(int)key];
        }

        public float trigVal { get; set; }
        public float grabVal { get; set; }


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
