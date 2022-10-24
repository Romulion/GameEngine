using System;
using System.Collections.Generic;
using System.Text;
using Valve.VR;
using OpenTK.Mathematics;

namespace Toys.VR
{
    public class VRSystem
    {
        internal CVRSystem VRContext;
        public VRControllerSystem controllerSystem { get; private set; }
        IntPtr RenderModel = IntPtr.Zero;
        readonly CVRCompositor compositor; 
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

        internal VRSystem()
        {
            if (!OpenVR.IsRuntimeInstalled())
                throw new Exception("VR Runtime not installed");
            if (!OpenVR.IsHmdPresent())
                throw new Exception("HMD not found");
            EVRInitError error = EVRInitError.None;
            VRContext = OpenVR.Init(ref error);
            if (error != EVRInitError.None)
            {
                Logger.Error(error);
                throw new Exception(error.ToString());
            }

            compositor = OpenVR.Compositor;
            RenderModel = OpenVR.GetGenericInterface(OpenVR.IVRRenderModels_Version, ref error);
            if (RenderModel == IntPtr.Zero)
                Logger.Error(error);

            
            controllerSystem = new VRControllerSystem(VRContext);

            leftTexture = new Texture_t();
            leftTexture.eType = ETextureType.OpenGL;
            leftTexture.eColorSpace = EColorSpace.Gamma;

            rightTexture = new Texture_t();
            rightTexture.eType = ETextureType.OpenGL;
            rightTexture.eColorSpace = EColorSpace.Gamma;
            VRContext.GetRecommendedRenderTargetSize(ref width , ref height);

            bound = new VRTextureBounds_t();
            bound.uMin = 0;
            bound.vMin = 0;
            bound.uMax = 1;
            bound.vMax = 1;

            //ETrackedPropertyError err = ETrackedPropertyError.TrackedProp_Success;
            //Console.WriteLine(vrContext.GetFloatTrackedDeviceProperty(0, ETrackedDeviceProperty.Prop_DisplayFrequency_Float, ref err));


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
            while (VRContext.PollNextEvent(ref vrEvent, 64))
                ProcessEvent(vrEvent);
            compositor.WaitGetPoses(render, game);
            controllerSystem.Update(render);

            var ctrans = CoreEngine.GetCamera.Node.GetTransform;
            var pos = controllerSystem.HMD.Position;
            pos.Y +=  0.4f;
            ctrans.Position = pos;
            ctrans.RotationQuaternion = controllerSystem.HMD.Rotation;
        
        }

        void ProcessEvent(VREvent_t vrEvent)
        {
            //Console.WriteLine((EVREventType) vrEvent.eventType);

            switch ((EVREventType)vrEvent.eventType)
            {
                case EVREventType.VREvent_StandingZeroPoseReset:
                    break;
            }

            ETrackedDeviceClass trackedDeviceClass = VRContext.GetTrackedDeviceClass(vrEvent.trackedDeviceIndex);
            if(trackedDeviceClass != ETrackedDeviceClass.Controller) {
	            return;
            }


            controllerSystem.ProcessInputEvent(vrEvent);            
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
