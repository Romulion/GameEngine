using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BulletSharp;
//using BulletSharp.Math;
using OpenTK;

namespace Toys
{
    class TestScript : ScriptingComponent
    {
        //DiscreteDynamicsWorld World;
        long frames = 1;
        double update = 0, render = 0;
        //TextBox text;
        //RigidBody Body;
        BoneController bc;

        void Awake()
        {
            var msd = (Animator)node.GetComponent<Animator>();
            if (msd != null)
                bc = msd.bones;
            //World = CoreEngine.pEngine.World;
        }

        void Start()
        {
            //Console.WriteLine(bc.GetBone(82).Bone2WorldInitial);
            //Console.WriteLine(bc.GetBone(81).Bone2WorldInitial);
            //Console.WriteLine(bc.GetBone(3).BoneMatrix);
            //bc.GetBone(19).SetTransform(new Quaternion(0, 0, 1.5f), new Vector3(0, 0, 0));
            //bc.GetBone(21).SetTransform(new Quaternion(0, 1.5f, 0), new Vector3(0, 0, 0));
            //bc.GetBone(22).SetTransform(new Quaternion(0, 1.5f, 0), new Vector3(0, 0, 0));
            //bc.UpdateSkeleton();
            /*
            CollisionShape shape = new SphereShape(0.1f);
            RigidBodyConstructionInfo rbInfo = new RigidBodyConstructionInfo(0f, new DefaultMotionState(Matrix.Translation(new Vector3(0, 1, 0))), shape, Vector3.Zero);
            Body = new RigidBody(rbInfo);
            Body.ActivationState = ActivationState.DisableDeactivation;
            Body.Friction = 1.5f;
            Body.SetDamping(0.99f, 0.99f);
            Body.Restitution = 0.1f;
            Body.SetCustomDebugColor(new Vector3(0, 1, 0));
            World.AddRigidBody(Body);

            CollisionShape shape1 = new SphereShape(0.1f);
            Vector3 inertia1 = shape1.CalculateLocalInertia(0.05f);
            RigidBodyConstructionInfo rbInfo1 = new RigidBodyConstructionInfo(.05f, new DefaultMotionState(Matrix.Translation(new Vector3(0f, 1.5f, 0))), shape1, inertia1);
            RigidBody Body1 = new RigidBody(rbInfo1);
            Body1.ActivationState = ActivationState.DisableDeactivation;
            Body1.Friction = 1.5f;
            Body1.SetDamping(0.99f, 0.99f);
            Body1.Restitution = 0.1f;
            Body1.SetCustomDebugColor(new Vector3(1, 0, 0));
            World.AddRigidBody(Body1);


            CollisionShape shape2 = new SphereShape(0.1f);
            Vector3 inertia2 = shape2.CalculateLocalInertia(0.05f);
            RigidBodyConstructionInfo rbInfo2 = new RigidBodyConstructionInfo(.05f, new DefaultMotionState(Matrix.Translation(new Vector3(0.6f, 1.5f, 0))), shape2, inertia2);
            RigidBody Body2 = new RigidBody(rbInfo2);
            Body2.ActivationState = ActivationState.DisableDeactivation;
            Body2.Friction = 1.5f;
            Body2.SetDamping(0.99f, 0.99f);
            Body2.Restitution = 0.1f;
            Body2.SetCustomDebugColor(new Vector3(1, 0, 0));
            World.AddRigidBody(Body2);

            Matrix Conn1 = Matrix.Translation(new Vector3(0f, .25f, 0));
            Matrix Conn11 = Matrix.Translation(new Vector3(0f, -.25f, 0));

            Generic6DofSpringConstraint jointSpring6 = new Generic6DofSpringConstraint(Body, Body1, Conn1, Conn11,true);
            jointSpring6.AngularLowerLimit = new Vector3(0, 0, dec2rad(0));
            jointSpring6.AngularUpperLimit = new Vector3(0, 0, dec2rad(0));
            jointSpring6.LinearLowerLimit = Vector3.Zero;
            jointSpring6.LinearUpperLimit = Vector3.Zero;
            jointSpring6.EnableSpring(0, false);
            jointSpring6.EnableSpring(1, true);
            jointSpring6.EnableSpring(2, false);
            //jointSpring6.SetStiffness(1, 39.478f);
            //jointSpring6.SetDamping(1, 39.478f);
            jointSpring6.EnableSpring(3, false);
            jointSpring6.EnableSpring(4, false);
            //jointSpring6.SetParam(ConstraintParam.Erp, .9f);
            //jointSpring6.SetParam(ConstraintParam.StopCfm, 0f);
           // jointSpring6.SetParam(ConstraintParam.Cfm, 0f);
            jointSpring6.EnableSpring(5, false);
            jointSpring6.SetEquilibriumPoint();
            World.AddConstraint(jointSpring6, true);
            


            Matrix Conn2 = Matrix.Translation(new Vector3(-0.5f, 0, 0));
            Matrix Conn22 = Matrix.Translation(new Vector3(0.3f, 0, 0));
            Generic6DofSpringConstraint jointSpring61 = new Generic6DofSpringConstraint(Body1, Body2, Conn2, Conn22,true);
            jointSpring61.AngularLowerLimit = Vector3.Zero;
            jointSpring61.AngularUpperLimit = Vector3.Zero;
            jointSpring61.LinearLowerLimit = Vector3.Zero;
            jointSpring61.LinearUpperLimit = Vector3.Zero;
            //jointSpring61.SetParam(ConstraintParam.StopCfm, 0f);
            //jointSpring61.SetParam(ConstraintParam.Cfm, 0f);
            jointSpring61.SetEquilibriumPoint();
            World.AddConstraint(jointSpring61, true);
            */
        }

        void Update()
        {
           // update++;
           // bc.GetBone(3).SetTransform(new Quaternion(0, 0, (float)(dec2rad(45) * Math.Cos(update * 3 * Math.PI / 180))), new Vector3(0, 0, 0));
            //update += .UpdateTime * 1000;
            //Matrix world = Body.WorldTransform;
            //world.M42 = (float)(1 + 0.5 * Math.Cos(update * 3 * Math.PI / 180));
            //Body.MotionState.SetWorldTransform(ref world);
            //Body.WorldTransform = world;
        }

        float dec2rad(int grad)
        {
           return ((float)grad / 180 * (float)Math.PI);
        }
    }
}
