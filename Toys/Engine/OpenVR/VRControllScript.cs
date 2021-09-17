using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Mathematics;

namespace Toys.VR
{
    public class VRControllScript : ScriptingComponent
    {
        Transform rHandpos;
        VRSystem vr;

        void Start()
        {
            vr = CoreEngine.vrSystem;
            CreateRHand();
        }
        void CreateRHand()
        {
            var rhand = new SceneNode();
            rhand.Name = "RHand";
            var size = new Vector3(0.03f, 0.03f, 0.15f);
            rhand.AddComponent(Debug.TestBox.CreateBox(size));
            rhand.GetTransform.Position = new Vector3(0, 2, -1f);
            var physBox = rhand.AddComponent<Physics.RigitBodyBox>();
            physBox.Size = size;
            physBox.IsKinematic = true;
            physBox.SetFlags((int)CollisionFilleters.All, (int)CollisionFilleters.All);
            //CoreEngine.MainScene.AddNode2Root(rhand);
            rhand.SetParent(CoreEngine.GetCamera.Node.Parent);
            
            rHandpos = rhand.GetTransform;
        }

        void Update()
        {
            var pos = vr.controllerSystem.controllers[0].pos;
            pos.Y += 0.4f;
            rHandpos.Position = pos;
            rHandpos.RotationQuaternion = vr.controllerSystem.controllers[0].rot;

            
            //Console.WriteLine(rHandpos.GlobalTransform);
        }
    }
}
