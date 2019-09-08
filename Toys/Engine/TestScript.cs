using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BulletSharp;
using BulletSharp.Math;

namespace Toys
{
    class TestScript : ScriptingComponent
    {
        DiscreteDynamicsWorld World;
        long frames = 1;
        double update = 0, render = 0;
        TextBox text;
        RigidBody Body;

        void Awake()
        {
            World = CoreEngine.pEngine.World;
        }

        void Start()
        {
            CollisionShape shape = new SphereShape(0.1f);
            RigidBodyConstructionInfo rbInfo = new RigidBodyConstructionInfo(0, new DefaultMotionState(Matrix.Translation(new Vector3(0, 1, 0))), shape, Vector3.Zero);
            Body = new RigidBody(rbInfo);
            Body.ActivationState = ActivationState.DisableDeactivation;
            Body.Friction = 1.5f;
            Body.SetDamping(0.99f, 0.99f);
            Body.Restitution = 0.1f;
            Body.SetCustomDebugColor(new Vector3(0, 1, 0));
            World.AddRigidBody(Body);


            CollisionShape shape1 = new SphereShape(0.1f);
            Vector3 inertia1 = shape1.CalculateLocalInertia(0.5f);
            RigidBodyConstructionInfo rbInfo1 = new RigidBodyConstructionInfo(0.5f, new DefaultMotionState(Matrix.Translation(new Vector3(0.3f, 1, 0))), shape1, inertia1);
            RigidBody Body1 = new RigidBody(rbInfo1);
            Body1.ActivationState = ActivationState.DisableDeactivation;
            Body1.Friction = 1.5f;
            Body1.SetDamping(0.99f, 0.99f);
            Body1.Restitution = 0.1f;
            Body1.SetCustomDebugColor(new Vector3(1, 1, 0));
            World.AddRigidBody(Body1);


            CollisionShape shape2 = new SphereShape(0.1f);
            Vector3 inertia2 = shape2.CalculateLocalInertia(0.5f);
            RigidBodyConstructionInfo rbInfo2 = new RigidBodyConstructionInfo(0.5f, new DefaultMotionState(Matrix.Translation(new Vector3(0.6f, 1, 0))), shape2, inertia2);
            RigidBody Body2 = new RigidBody(rbInfo2);
            Body2.ActivationState = ActivationState.DisableDeactivation;
            Body2.Friction = 1.5f;
            Body2.SetDamping(0.99f, 0.99f);
            Body2.Restitution = 0.1f;
            Body2.SetCustomDebugColor(new Vector3(1, 0, 0));
            World.AddRigidBody(Body2);

            Matrix Conn1 = Matrix.Translation(new Vector3(0.15f, 0, 0));
            Matrix Conn11 = Matrix.Translation(new Vector3(-0.15f, 0, 0));
            Generic6DofSpring2Constraint jointSpring6 = new Generic6DofSpring2Constraint(Body, Body1, Conn1, Conn11);
            jointSpring6.AngularLowerLimit = Vector3.Zero;
            jointSpring6.AngularUpperLimit = Vector3.Zero;
            jointSpring6.LinearLowerLimit = Vector3.Zero;
            jointSpring6.LinearUpperLimit = Vector3.Zero;
            World.AddConstraint(jointSpring6, true);

            Matrix Conn2 = Matrix.Translation(new Vector3(0.15f, 0, 0));
            Matrix Conn22 = Matrix.Translation(new Vector3(-0.15f, 0, 0));
            Generic6DofSpring2Constraint jointSpring61 = new Generic6DofSpring2Constraint(Body1, Body2, Conn2, Conn22);
            jointSpring61.AngularLowerLimit = Vector3.Zero;
            jointSpring61.AngularUpperLimit = Vector3.Zero;
            jointSpring61.LinearLowerLimit = Vector3.Zero;
            jointSpring61.LinearUpperLimit = Vector3.Zero;
            World.AddConstraint(jointSpring61, true);
        }

            void Update()
        {
            update++;
            //update += .UpdateTime * 1000;
            Matrix world = Matrix.Identity;
            world.M42 = (float)(1 + 0.5 * Math.Cos(update * 10 * Math.PI / 180));
            Body.WorldTransform = world;
        }
    }
}
