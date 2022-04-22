using System;
using System.Collections.Generic;
using System.Text;
using Valve.VR;
using OpenTK.Mathematics;

namespace Toys.VR
{

    public class VRControllerSystem
    {
        //event 
        CVRSystem vrContext;
        public ControllerData[] controllers;
        TrackerData[] trackers;
        ETrackedPropertyError trackError;
        uint vrControllerStateSize = 0;
        bool inDrawingMode = true;
        const string setName = "/actions/player";
        VRActiveActionSet_t[] setList;

        InputAnalogActionData_t analogData = new InputAnalogActionData_t();
        InputDigitalActionData_t digitalData = new InputDigitalActionData_t();
        InputPoseActionData_t posData = new InputPoseActionData_t();
        uint analogDataSize;
        uint digitalDataSize;
        uint posDataSize;
        uint setSize;

        public HMDData HMD { get; private set; }

        public VRControllerSystem(CVRSystem context)
        {


            trackError = ETrackedPropertyError.TrackedProp_Success;
            vrContext = context;
            HMD = new HMDData();
            HMD.Id = 0;
            controllers = new ControllerData[2] { new ControllerData(), new ControllerData() };
            //ParseTrackingFrame();
            ResetHMDLocation();
            SetBindingsFromManifest();

            Update(null);
            ResetHMDLocation();
            vrControllerStateSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(VRControllerState_t));
        }

        void SetBindingsFromManifest()
        {
            
            OpenVR.Input.SetActionManifestPath(ResourcesManager.GetAbsoluteAssetDirectory() + @"Other\actions.json");

            setList = new VRActiveActionSet_t[] { new VRActiveActionSet_t() };
            OpenVR.Input.GetActionSetHandle(setName, ref setList[0].ulActionSet);

            controllers[0].controllerRole = ControllerRole.Left;
            OpenVR.Input.GetInputSourceHandle("/user/hand/left", ref controllers[0].sourceId);
            controllers[1].controllerRole = ControllerRole.Right;
            OpenVR.Input.GetInputSourceHandle("/user/hand/right", ref controllers[1].sourceId);
            
            
            for (int i = 0; i < 2; i++)
            {
                OpenVR.Input.GetActionHandle("/actions/player/in/pause", ref controllers[i].button1PressId);
                OpenVR.Input.GetActionHandle("/actions/player/in/menu", ref controllers[i].button2PressId);
                OpenVR.Input.GetActionHandle("/actions/player/in/grab", ref controllers[i].grabPressId);
                OpenVR.Input.GetActionHandle("/actions/player/in/action", ref controllers[i].triggerPressId);
                OpenVR.Input.GetActionHandle("/actions/player/in/trackpadpress", ref controllers[i].stickerPressId);
                OpenVR.Input.GetActionHandle("/actions/player/in/trackpad", ref controllers[i].stickerPosId);
                OpenVR.Input.GetActionHandle("/actions/player/in/pose", ref controllers[i].poseId);
            }


            

            
            setSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(VRActiveActionSet_t));
            analogDataSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(InputAnalogActionData_t));
            digitalDataSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(InputDigitalActionData_t));
            posDataSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(InputPoseActionData_t));
            
        }

        internal void Update(TrackedDevicePose_t[] pose)
        {
            
            OpenVR.Input.UpdateActionState(setList, setSize);
            for (int i = 0; i < 2; i++)
            {
                Array.Copy(controllers[i].triggerState, controllers[i].triggerPrevState, controllers[i].triggerPrevState.Length);
                OpenVR.Input.GetDigitalActionData(controllers[i].button1PressId, ref digitalData, digitalDataSize, controllers[i].sourceId);
                controllers[i].SetBoolValue(ControllerButton.Button1,digitalData.bState);
                OpenVR.Input.GetDigitalActionData(controllers[i].button2PressId, ref digitalData, digitalDataSize, controllers[i].sourceId);
                controllers[i].SetBoolValue(ControllerButton.Button2, digitalData.bState);
                OpenVR.Input.GetDigitalActionData(controllers[i].triggerPressId, ref digitalData, digitalDataSize, controllers[i].sourceId);
                controllers[i].SetBoolValue(ControllerButton.Trigger, digitalData.bState);
                OpenVR.Input.GetDigitalActionData(controllers[i].grabPressId, ref digitalData, digitalDataSize, controllers[i].sourceId);
                controllers[i].SetBoolValue(ControllerButton.Grab, digitalData.bState);
                OpenVR.Input.GetDigitalActionData(controllers[i].stickerPressId, ref digitalData, digitalDataSize, controllers[i].sourceId);
                controllers[i].SetBoolValue(ControllerButton.Stick, digitalData.bState);
                OpenVR.Input.GetAnalogActionData(controllers[i].stickerPosId, ref analogData, analogDataSize, controllers[i].sourceId);
                controllers[i].stick.X = analogData.x;
                controllers[i].stick.Y = analogData.y;
                OpenVR.Input.GetPoseActionDataForNextFrame(controllers[i].poseId, ETrackingUniverseOrigin.TrackingUniverseStanding ,ref posData, posDataSize, controllers[i].sourceId);
                var mat = ConvertMatrix(posData.pose.mDeviceToAbsoluteTracking);
                controllers[i].pos = mat.ExtractTranslation();
                controllers[i].rot = mat.ExtractRotation();
            }
            HMDCoords();
            //OpenVR.Input.GetAnalogActionData(val, ref analog, (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(InputAnalogActionData_t)), 0);
        }

        internal void ProcessInputEvent(VREvent_t vrEvent)
        {
            /*
            int controllerIndex = 0; //The index of the controllers[] array that corresponds with the controller that had a buttonEvent
            for (int i = 0; i < 2; i++) //Iterates across the array of controllers
            {
                ControllerData pController = controllers[i];

                if (pController.deviceId == vrEvent.trackedDeviceIndex) //This tests to see if the current controller from the loop is the same from the event
                    controllerIndex = i;
            }

            ControllerData pC = controllers[controllerIndex]; //The pointer to the ControllerData struct
            //Console.WriteLine(OpenVR.Butt vrEvent.data.controller.button);
            if (vrEvent.data.controller.button == (uint)EVRButtonId.k_EButton_ApplicationMenu //Test if the ApplicationButton was pressed
                && vrEvent.eventType == (uint)EVREventType.VREvent_ButtonUnpress)              //Test if the button is being released (the action happens on release, not press)

            {
                inDrawingMode = !inDrawingMode;
            }
            if (inDrawingMode) { }
            */
        }


        void IterateAssignIds()
        {
            /*
            var trackers = new List<TrackerData>();
            //Un-assigns the deviceIds and hands of controllers. If they are truely connected, will be re-assigned later in this function
            controllers[0].deviceId = -1;
            controllers[1].deviceId = -1;
            controllers[0].hand = -1;
            controllers[1].hand = -1;

            int numControllersInitialized = 0;
            var errProp = new ETrackedPropertyError();
            var str = new StringBuilder();
            for (uint i = 0; i < OpenVR.k_unMaxTrackedDeviceCount; i++)  // Iterates across all of the potential device indicies
            {
                if (!vrContext.IsTrackedDeviceConnected(i))
                    continue; //Doesn't use the id if the device isn't connected

                //vr_pointer points to the VRSystem that was in init'ed in the constructor.
                ETrackedDeviceClass trackedDeviceClass = vrContext.GetTrackedDeviceClass(i);

                //Finding the type of device
                if (trackedDeviceClass == ETrackedDeviceClass.HMD)
                {
                    HMD.Id = (int)i;
                }
                //Get availible controllers
                else if (trackedDeviceClass == ETrackedDeviceClass.Controller && numControllersInitialized < 2)
                {
                    ControllerData pC = controllers[numControllersInitialized];

                    int sHand = -1;

                    ETrackedControllerRole role = vrContext.GetControllerRoleForTrackedDeviceIndex(i);
                    if (role == ETrackedControllerRole.Invalid) //Invalid hand is actually very common, always need to test for invalid hand (lighthouses have lost tracking)
                        sHand = 0;
                    else if (role == ETrackedControllerRole.LeftHand)
                        sHand = 1;
                    else if (role == ETrackedControllerRole.RightHand)
                        sHand = 2;
                    pC.hand = sHand;
                    pC.deviceId = (int)i;

                    //Console.WriteLine("{0} {1} {2}", i, pC.padX, pC.padY);

                    //Used to get/store property ids for the xy of the pad and the analog reading of the trigger
                    for (int x = 0; x < OpenVR.k_unControllerStateAxisCount; x++)
                    {
                        int prop = vrContext.GetInt32TrackedDeviceProperty((uint)pC.deviceId,
                            (ETrackedDeviceProperty.Prop_Axis0Type_Int32 + x), ref trackError);
                        if (prop == (int)EVRControllerAxisType.k_eControllerAxis_Trigger)
                            pC.idtrigger = x;
                        else if (prop == (int)EVRControllerAxisType.k_eControllerAxis_TrackPad || prop == (int)EVRControllerAxisType.k_eControllerAxis_Joystick)
                            pC.idpad = x;

                        //Console.WriteLine("{0} {1} {2}", i, x, (EVRControllerAxisType)prop);
                    }


                    vrContext.GetStringTrackedDeviceProperty((uint)pC.deviceId, ETrackedDeviceProperty.Prop_RenderModelName_String, str, 100, ref errProp);
                    pC.RenderModelName = str.ToString();

                    //OpenVR.RenderModels.LoadRenderModel_Async()
                    numControllersInitialized++; //Increment this count so that the other controller gets initialized after initializing this one
                }
                //Get availible trackers
                else if (trackedDeviceClass == ETrackedDeviceClass.GenericTracker)
                {
                    var tracker = new TrackerData();
                    trackers.Add(tracker);
                    tracker.deviceId = (int)i;
                }
                
            }

            this.trackers = trackers.ToArray();
            */
        }

        void SetHands()
        {
            /*
            for (int z = 0; z < 2; z++)
            {
                ControllerData pC = controllers[z];
                if (pC.deviceId < 0 || !vrContext.IsTrackedDeviceConnected((uint)pC.deviceId))
                    continue;
                int sHand = -1;
                //Invalid hand is actually very common, always need to test for invalid hand (lighthouses have lost tracking)
                ETrackedControllerRole role = vrContext.GetControllerRoleForTrackedDeviceIndex((uint)pC.deviceId);
                if (role == ETrackedControllerRole.Invalid)
                    sHand = 0;
                else if (role == ETrackedControllerRole.LeftHand)
                    sHand = 1;
                else if (role == ETrackedControllerRole.RightHand)
                    sHand = 2;
                pC.hand = sHand;
            }
            */
        }

        internal void ParseTrackingFrame()
        {
            /*
            //Runs the iterateAssignIds() method if...
            if (HMD.Id < 0 ||                     // HMD id not yet initialized
                controllers[0].deviceId < 0 ||       // One of the controllers not yet initialized
                controllers[1].deviceId < 0 ||
                controllers[0].deviceId == controllers[1].deviceId ||  //Both controllerData structs store the same deviceId
                controllers[0].hand == controllers[1].hand) //Both controllerData structs are the same hand
                //||          
                //(CoreEngine.time.TimeFromStart / 60000) > minuteCount)                    //It has been a minute since last init time
            {
                //minuteCount = (int)(CoreEngine.time.TimeFromStart / 60000);
                IterateAssignIds();
            }
            HMDCoords();
            ControllerCoords();
            TrackerCoords();     
            */
        }

        void HMDCoords()
        {
            if (!vrContext.IsTrackedDeviceConnected((uint)HMD.Id))
                return;

            //TrackedDevicePose_t struct is a OpenVR struct. See line 180 in the openvr.h header.
            TrackedDevicePose_t[] trackedDevicePose = new TrackedDevicePose_t[1];
            //if (vrContext.IsInputFocusCapturedByAnotherProcess())
            //    printf("\nINFO--Input Focus by Another Process");
            vrContext.GetDeviceToAbsoluteTrackingPose(ETrackingUniverseOrigin.TrackingUniverseStanding, 0f, trackedDevicePose);
            var mat = ConvertMatrix(trackedDevicePose[0].mDeviceToAbsoluteTracking);
            HMD.Position = GetPosition(mat);
            HMD.Rotation = GetRotation(mat);
            //Console.WriteLine(111111);
            //Console.WriteLine(mat);
        }

        void ControllerCoords()
        {
            /*
            SetHands();
            
            if (doRumbleNow)
            {
                rumbleMsOffset = cpMillis();
                doRumbleNow = false;
            }

            TrackedDevicePose_t trackedDevicePose = new TrackedDevicePose_t();
            VRControllerState_t controllerState = new VRControllerState_t();

            //Arrays to contain information about the results of the button state sprintf call 
            //  so that the button state information can be printed all on one line for both controllers
            bool[] isOk = new bool[2];

            //Stores the number of times 150ms have elapsed (loops with the % operator because 
            //  the "cylinder count" rumbling starts when indexN is one).
            //int indexN = ((cpMillis() - rumbleMsOffset) / 150) % (125);

            //Loops for each ControllerData struct
            for (int i = 0; i < 2; i++)
            {
                isOk[i] = false;
                ControllerData pC = controllers[i];

                if (pC.deviceId < 0 ||
                    !vrContext.IsTrackedDeviceConnected((uint)pC.deviceId) ||
                    pC.hand < 0)
                    continue;

                vrContext.GetControllerStateWithPose(ETrackingUniverseOrigin.TrackingUniverseStanding, (uint)pC.deviceId, ref controllerState, vrControllerStateSize, ref trackedDevicePose);
                var mat = ConvertMatrix(trackedDevicePose.mDeviceToAbsoluteTracking);
                pC.pos = GetPosition(mat);
                pC.rot = GetRotation(mat);
                
                pC.isValid = trackedDevicePose.bPoseIsValid && trackedDevicePose.bDeviceIsConnected;

                int t = pC.idtrigger;
                int p = pC.idpad;

                //This is the call to get analog button data from the controllers
                if (t == 0)
                    pC.trigVal = controllerState.rAxis0.x;
                else if (t == 1)
                    pC.trigVal = controllerState.rAxis1.x;
                else if (t == 2)
                    pC.trigVal = controllerState.rAxis2.x;
                else if (t == 3)
                    pC.trigVal = controllerState.rAxis3.x;
                else if (t == 4)
                    pC.trigVal = controllerState.rAxis4.x;

                if (p == 0)
                {
                    pC.padX = controllerState.rAxis0.x;
                    pC.padY = controllerState.rAxis0.y;
                }
                else if (p == 1)
                {
                    pC.padX = controllerState.rAxis1.x;
                    pC.padY = controllerState.rAxis1.y;
                }
                else if (p == 2)
                {
                    pC.padX = controllerState.rAxis2.x;
                    pC.padY = controllerState.rAxis2.y;
                }
                else if (p == 3)
                {
                    pC.padX = controllerState.rAxis3.x;
                    pC.padY = controllerState.rAxis3.y;
                }
                else if (p == 4)
                {
                    pC.padX = controllerState.rAxis4.x;
                    pC.padY = controllerState.rAxis4.y;
                }

                isOk[i] = true;

                //Console.WriteLine("{0} {1} {2}", i, pC.padX, pC.padY);
                /*
                //The following block controlls the rumbling of the controllers
                if (!inDrawingMode) //Will iterate across all cylinders if in sensing mode
                    for (int x = 0; x < MAX_CYLINDERS; x++)
                    {
                        Cylinder* currCy = cylinders[x];
                        if (currCy->hasInit &&
                            currCy->isInside(pC->pos.v[0], pC->pos.v[1], pC->pos.v[2]))
                            vrContext.TriggerHapticPulse(pC->deviceId, pC->idpad, 500); //Vibrates if the controller is colliding with the cylinder bounds
                    }
                if (inDrawingMode && indexN % 3 == 0 && indexN < (cylinderIndex + 1) * 3) //Vibrates the current cylinderIndex every thirty seconds or so
                    vrContext.TriggerHapticPulse(pC->deviceId, pC->idpad, 300);         //  see the definition of indexN above before the for loop
                
            }
            */
        }

        void TrackerCoords()
        {
            TrackedDevicePose_t trackedDevicePose = new TrackedDevicePose_t();
            VRControllerState_t controllerState = new VRControllerState_t();

            for (int i = 0; i < trackers.Length; i++)
            {
                TrackerData pT = trackers[i];

                if (pT.deviceId < 0 ||
                    !vrContext.IsTrackedDeviceConnected((uint)pT.deviceId))
                    continue;

                unsafe
                {
                    vrContext.GetControllerStateWithPose(ETrackingUniverseOrigin.TrackingUniverseStanding, (uint)pT.deviceId, ref controllerState, vrControllerStateSize, ref trackedDevicePose);
                }
                var mat = ConvertMatrix(trackedDevicePose.mDeviceToAbsoluteTracking);
                pT.position = GetPosition(mat);
                pT.rotation = GetRotation(mat);
                pT.isValid = trackedDevicePose.bPoseIsValid;
            }
        }
        internal Vector3 GetPosition(Matrix4 matrix)
        {
            return matrix.ExtractTranslation();
        }

        internal Matrix4 ConvertMatrix (HmdMatrix34_t matrix)
        {
            
            var mat4 = new Matrix4
            (
                matrix.m0, matrix.m4, matrix.m8, 0,
                matrix.m1, matrix.m5, matrix.m9, 0,
                matrix.m2, matrix.m6, matrix.m10, 0,
                matrix.m3, matrix.m7, matrix.m11, 0
            );
            
            return mat4;
        }

        internal Quaternion GetRotation(Matrix4 matrix)
        {
            return matrix.ExtractRotation();
        }

        internal void ResetHMDLocation()
        {
            HMD.StartPosition = HMD.Position;
            HMD.StartRotation = HMD.Rotation;
        }

        void UpdateControllers()
        {
            //vrContext.GetControllerStateWithPose(ETrackingUniverseOrigin.TrackingUniverseStanding,)
        }
    }
}

