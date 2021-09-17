using System;
using System.Collections.Generic;
using System.Text;
using Valve.VR;
using OpenTK.Mathematics;

namespace Toys.VR
{
    public class VRSystem
    {
        CVRSystem vrContext;
        public VRControllerSystem controllerSystem { get; private set; }
        IntPtr RenderModel = IntPtr.Zero;
        CVRCompositor compositor; 
        Texture_t leftTexture;
        Texture_t rightTexture;
        public uint height, width;
        VRTextureBounds_t bound;
        TrackedDevicePose_t[] game = new TrackedDevicePose_t[3];
        TrackedDevicePose_t[] render = new TrackedDevicePose_t[3];
        /// <summary>
        /// IPD in mm
        /// </summary>
        public int IPD = 68;
        Transform rHandpos;

        internal VRSystem()
        {
            if (!OpenVR.IsRuntimeInstalled())
                throw new Exception("VR Runtime not installed");
            if (!OpenVR.IsHmdPresent())
                throw new Exception("HMD not found");
            EVRInitError error = EVRInitError.None;
            vrContext = OpenVR.Init(ref error);
            if (error != EVRInitError.None)
                Console.WriteLine(error);
            
            compositor = OpenVR.Compositor;
            RenderModel = OpenVR.GetGenericInterface(OpenVR.IVRRenderModels_Version, ref error);
            if (RenderModel == IntPtr.Zero);
            {
                Console.WriteLine(error);
            }
            
            controllerSystem = new VRControllerSystem(vrContext);

            leftTexture = new Texture_t();
            leftTexture.eType = ETextureType.OpenGL;
            leftTexture.eColorSpace = EColorSpace.Gamma;

            rightTexture = new Texture_t();
            rightTexture.eType = ETextureType.OpenGL;
            rightTexture.eColorSpace = EColorSpace.Gamma;
            vrContext.GetRecommendedRenderTargetSize(ref height, ref width);

            bound = new VRTextureBounds_t();
            bound.uMin = 0;
            bound.vMin = 0;
            bound.vMax = 1;
            bound.uMax = 1;

            /*
            var errProp = new ETrackedPropertyError();
            var str = new StringBuilder();
            vrContext.GetStringTrackedDeviceProperty(OpenVR.k_unTrackedDeviceIndex_Hmd, ETrackedDeviceProperty.Prop_TrackingSystemName_String, str, 100, ref errProp);
            */
        }



        internal void Update()
        {
            
            if (CoreEngine.GetCamera.Width != (int)width)
                CoreEngine.ActiveCore.Resize((int)width, (int)height);
            

            var vrEvent =  new VREvent_t();
            while (vrContext.PollNextEvent(ref vrEvent, 64))
                ProcessEvent(vrEvent);

            compositor.WaitGetPoses(render, game);
            controllerSystem.Update(render);

            var ctrans = CoreEngine.GetCamera.Node.GetTransform;
            var pos = controllerSystem.HMD.Position;
            pos.Y +=  0.4f;
            ctrans.Position = pos;
            ctrans.RotationQuaternion = controllerSystem.HMD.Rotation;

            //Console.WriteLine(controllerSystem.HMD.Position);
            //Console.WriteLine(controllerSystem.HMD.Rotation.ToEulerAngles());
        }

        void ProcessEvent(VREvent_t vrEvent)
        {
            //Console.WriteLine((EVREventType) vrEvent.eventType);

            switch ((EVREventType)vrEvent.eventType)
            {
                case EVREventType.VREvent_StandingZeroPoseReset:
                    break;
            }

            ETrackedDeviceClass trackedDeviceClass = vrContext.GetTrackedDeviceClass(vrEvent.trackedDeviceIndex);
            if(trackedDeviceClass != ETrackedDeviceClass.Controller) {
	            return;
            }


            controllerSystem.ProcessInputEvent(vrEvent);
            /*
            if (vrEvent.eventType == (uint)EVREventType.VREvent_ButtonPress)
            {

            }
            else if (vrEvent.eventType == (uint)EVREventType.VREvent_ButtonTouch)
            {

            }
            else if (vrEvent.eventType == (uint)EVREventType.VREvent_ButtonUnpress)
            {

            }
            else if (vrEvent.eventType == (uint)EVREventType.VREvent_ButtonUntouch)
            {

            }
            else if (vrEvent.eventType == (uint)EVREventType.VREvent_TouchPadMove)
            {
                //Console.WriteLine((EVRButtonId)vrEvent.data.touchPadMove.fValueXFirst);
                //Console.WriteLine((EVRButtonId)vrEvent.data.touchPadMove.fValueYFirst);
            }
            */
            //Console.WriteLine(compositor.GetLastFrameRenderer());


            
        }

        internal void SetLeftEye(int textureID)
        {
            leftTexture.handle = (IntPtr)textureID;
            compositor.Submit(EVREye.Eye_Left, ref leftTexture, ref bound, EVRSubmitFlags.Submit_Default);
        }

        internal void SetRightEye(int textureID)
        {
            rightTexture.handle = (IntPtr)textureID;
            compositor.Submit(EVREye.Eye_Right, ref rightTexture, ref bound, EVRSubmitFlags.Submit_Default);
        }

        internal void Exit()
        {
            OpenVR.Shutdown();
        }
    }
}
