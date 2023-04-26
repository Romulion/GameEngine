using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Mathematics;
using Toys;
using Toys.VR;

namespace ModelViewer
{
    public class VRControllScript : ScriptingComponent
    {
        Transform rHandpos;
        VRSystem vr;
        double time = 0;
        float progression = 0;
        bool inCollision = false;
        bool regress = false;
        float progressResolv = 1 / 200f;
        Morph[] morph1 = new Morph[2];
        NPCController controller;
        CharacterControllPlayer ccp;
        bool say = false;
        PhysicsManager man;

        void Start()
        {
            vr = CoreEngine.VRSystem;
            CreateRHand();

            
            var node = CoreEngine.MainScene.FindByName("Michelle.Seifuku")[0];
            
            var drwr = node.GetComponent<MeshDrawerRigged>();
            drwr.Materials[38].RenderDirrectives.IsRendered = true;
            drwr.Materials[39].RenderDirrectives.IsRendered = true;
            drwr.Materials[36].RenderDirrectives.IsRendered = false;
            //morph = drwr.Morphes[144];


            man = node.GetComponent<PhysicsManager>();

            controller = node.GetComponent<NPCController>();


            ccp =  Node.Parent.GetComponent<CharacterControllPlayer>();

        }
        void CreateRHand()
        {
            var rhand = new SceneNode();
            rhand.Name = "LHand";
            var size = new Vector3(0.03f, 0.03f, 0.15f);
            rhand.AddComponent(Toys.Debug.TestBox.CreateBox(size));
            rhand.GetTransform.Position = new Vector3(0, 2, -1f);
            var physBox = new Toys.Physics.RigitBodyBox(size);
            rhand.AddComponent(physBox);
            physBox.IsKinematic = true;
            physBox.SetFlags((int)CollisionFilleters.MMDAll,  (int)CollisionFilleters.MMDAll);
            physBox.OnHit += callback;
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

            //Play collision ON enter
            if (say)
            {
                controller.AI.PlayVoice(24);
                say = false;
            }

            if (CoreEngine.VRSystem.controllerSystem.controllers[(int)ControllerRole.Left].IsButtonPressed(ControllerButton.Button1))
            {
                controller.navigationController.AnimController.SetBool("standLean", !controller.navigationController.AnimController.GetBool("standLean"));
            }

            if ((CoreEngine.ISystem.CurrentContext | InputContext.Main) > 0)
            {
                var dir = new Vector4(0, 0, -vr.controllerSystem.controllers[0].stick.Y * 0.2f, 0);
                dir *= CoreEngine.GetCamera.Node.GetTransform.GetTransform;

                ccp.Walk(dir.Xyz);
            }

            if (!inCollision && progression > 0 && !regress)
            {
                if (time == 0)
                    time = CoreEngine.Time.TimeFromStart;
                if (CoreEngine.Time.TimeFromStart - time > 5000)
                {
                    regress = true;
                    time = 0;
                }
            }
            else if (regress)
            {
                if (progression > 0)
                    progression -= 2 * progressResolv;
                else
                    regress = false;
            }
            inCollision = false;

            SetProgress();
            
        }
        

        void SetProgress()
        {

            //morph1[0].MorphDegree = progression;
            //morph1[1].MorphDegree = progression;
            //(man.joints[0].Constraint as BulletSharp.Generic6DofSpring2Constraint).SetBounce(3, 50);
        }

        void callback(CollisionData data)
        {
            if (data.Body.UserIndex2 == 17)
            {
                inCollision = true;
                if (progression <= 0)
                    say = true;
                if (progression < 1)
                    progression += progressResolv;
            }
        }
    }
}
