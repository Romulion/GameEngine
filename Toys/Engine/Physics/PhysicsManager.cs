using System;
using BulletSharp;
using BulletSharp.Math;
using System.Text;

namespace Toys
{
    public class PhysicsManager
    {
        public delegate void BoneBodySyncer(OpenTK.Matrix4 world);
        RigitBodyBone[] rigitBodies;
        AnimController bones;
        Joint[] joints;
        public BoneBodySyncer prePhysics;
        public BoneBodySyncer postPhysics;
        Transformation worldTrans;

		public DiscreteDynamicsWorld World { get; private set; }

        public PhysicsManager(RigitContainer[] rigits, JointContainer[] jcons, AnimController bons, Transformation trans)
        {
            //setup world physics
            bones = bons;
            worldTrans = trans;
            //instalize delegates
            prePhysics = (m) => { };
            postPhysics = (m) => { };
            World = CoreEngine.pEngine.World;
            InstalizeRigitBody(rigits);
            InstalizeJoints(jcons);
            //CreateGeneric6DofSpringConstraint();
        }

        void InstalizeRigitBody(RigitContainer[] rigits)
        {
            rigitBodies = new RigitBodyBone[rigits.Length];
            for (int i = 0; i < rigits.Length; i++)
            {

                //if (i != 2 && (i < 103 || i > 104))
                //    continue;

                rigitBodies[i] = new RigitBodyBone(rigits[i]);

                //skipping bone binding for no index riggs (ushort indexes only)
                if (rigits[i].BoneIndex != ushort.MaxValue)
                {
                    rigitBodies[i].bone = rigits[i].BoneIndex;
                    rigitBodies[i].acon = bones;
                    
                    if (rigits[i].Phys == PhysType.FollowBone || rigits[i].Phys == PhysType.GravityBone)
                        prePhysics += rigitBodies[i].SyncBone2Body;

                    if (rigits[i].Phys == PhysType.GravityBone || rigits[i].Phys == PhysType.Gravity)
                        postPhysics += rigitBodies[i].SyncBody2Bone;
                       
                }
                //World.AddRigidBody(rigitBodies[i].Body, rigits[i].GroupId, (short)(-1 ^ rigits[i].NonCollisionGroup));
                //Console.WriteLine("{0} {1} {2}",i, Convert.ToString((int)Math.Pow(2, rigits[i].GroupId), 2).PadLeft(16,'0'), Convert.ToString(rigits[i].NonCollisionGroup, 2).PadLeft(16, '0'));
                World.AddRigidBody(rigitBodies[i].Body, (int)Math.Pow(2, rigits[i].GroupId), rigits[i].NonCollisionGroup);
                //Console.WriteLine(rigitBodies[i].Body.WorldTransform);
                //World.AddRigidBody(rigitBodies[i].Body);

            }
        }

        void InstalizeJoints(JointContainer[] jcons)
        {
            joints = new Joint[jcons.Length];
            for (int i = 0; i < jcons.Length; i++)
            {

                //if (i < 80 || i > 81)
                //    continue;
                joints[i] = new Joint(jcons[i], rigitBodies);
                World.AddConstraint(joints[i].joint, true);

            }
        }

        public void Update()
        {
            prePhysics(worldTrans.globalTransform);
        }

        public void PostUpdate()
        {
            var worldInverted = worldTrans.globalTransform;
            worldInverted.Invert();
            postPhysics(worldInverted);
        }

        //test
        private void CreateGeneric6DofSpringConstraint()
        {
            float m_pi = (float)Math.PI;
            //head
            //var shape1 = new CapsuleShape(0.07216073f, .5224448f);
            //RigidBody bodyA = LocalCreateRigidBody(0, Matrix.RotationYawPitchRoll(0, 179.7769f / 180 * m_pi, -1.5987f / 180 * m_pi) * Matrix.Translation(0.08128397f, .3570998f, 0.004173893f), shape1);
            //bodyA.ActivationState = ActivationState.DisableDeactivation;

            //bone1
            /*
            var shape2 = new CapsuleShape(0.007721505f, 0.04023713f);
            var startTransform = Matrix.RotationYawPitchRoll(1.3789f / 180 * m_pi, 1.4505f / 180 * m_pi, 44.6852f / 180 * m_pi) * Matrix.Translation(.1406221f, .1583128f, 0.004318491f);
            RigidBody body2 = LocalCreateRigidBody(0.1f, startTransform, shape2);
            //bodyA.ActivationState = ActivationState.DisableDeactivation;
            body2.Friction = 0.5f;
            body2.SetDamping(0.99f, 0.99f);
            postPhysics += (world) =>
            {
                var mat = world * RigitBodyBone.GetMat(startTransform).Inverted() * RigitBodyBone.GetMat(body2.WorldTransform);
                bones.SetTransform(207, mat);
            };
            */
            RigidBody bodyA = rigitBodies[2].Body;
            prePhysics += rigitBodies[2].SyncBone2Body;
            //postPhysics += rigitBodies[2].SyncBody2Bone;
            World.AddRigidBody(bodyA);

            RigidBody body2 = rigitBodies[103].Body;
            postPhysics += rigitBodies[103].SyncBody2Bone;
            World.AddRigidBody(body2);
            /*
            Console.WriteLine(bodyA.WorldTransform);
            Console.WriteLine(body2.WorldTransform);
            */
            //Matrix frameInAs = Matrix.
            /*
            Matrix frameInA = new Matrix(0.8871775f, 0.4605389f, .02863805f, 0,
                -0.440339f, 0.8635458, -0.2457442f, 0,
                -0.137905, 0.2054083, 0.9689116f, 0,
                 0.1151779f, 0.195883f, 0.1906938f, 1);
                 */
            Matrix frameInA = new Matrix()
            {
                M11 = -0.9339796f,
                M12 = -0.03138809f,
                M13 = 0.3559456f,
                M21 = 0.3562262f,
                M22 = -0.003663362f,
                M23 = 0.9343928f,
                M31 = -0.02802483f,
                M32 = 0.9995007f,
                M33 = 0.01460284f,
                M44 = 1
            };
            frameInA *= Matrix.Translation(0.04061189f, 0.185695f, -0.0001211387f);

            Matrix frameInB = new Matrix()
            {
                M11 = -0.6620927f,
                M12 = 0.6469174f,
                M13 = -0.3783269f,
                M21 = 0.2595404f,
                M22 = -0.2756506f,
                M23 = -0.9255569f,
                M31 = -0.7030449f,
                M32 = -0.7109956f,
                M33 = 0.01460496f,
                M44 = 1
            };
            frameInB *= Matrix.Translation(0.0004077288f, 0.01971534f, 6.237165E-06f);
            // Matrix frameInA = Matrix.RotationYawPitchRoll(0.2483914f, 0.0286419f, -0.4788151f) * Matrix.Translation(0.1151779f, 0.195883f, 0.1906938f);
            //Matrix frameInB = Matrix.RotationYawPitchRoll(-0.2461227f, -0.0162994f, 0.4444541f) * Matrix.Translation(0.001742112f, 0.04157462f, -0.0009270776f);

            Console.WriteLine(frameInA);
            Console.WriteLine(frameInB);
            var generic6DofSpring = new Generic6DofSpring2Constraint(bodyA, body2, frameInA, frameInB)
            {
                AngularLowerLimit = new Vector3(-20f / 180 * m_pi, -20f / 180 * m_pi, -20f / 180 * m_pi),
                AngularUpperLimit = new Vector3(20f / 180 * m_pi, 20f / 180 * m_pi, 20f / 180 * m_pi),
                // DebugDrawSize = 5
            };



            World.AddConstraint(generic6DofSpring, true);

            RigidBody body3 = rigitBodies[104].Body;
            postPhysics += rigitBodies[104].SyncBody2Bone;
            World.AddRigidBody(body3);
            //bone2
            /*
            var shape3 = new CapsuleShape(0.01292619f, 0.06735901f);
            var startTransform1 = Matrix.RotationYawPitchRoll(-12.8452f / 180 * m_pi, -5.8773f / 180 * m_pi, 24.8455f / 180 * m_pi) * Matrix.Translation(.1654096f, .1124614f, 0.004938241f);
            RigidBody body3 = LocalCreateRigidBody(0.1f, startTransform1, shape3);
            body3.ActivationState = ActivationState.DisableDeactivation;
            body3.Friction = 0.5f;
            body3.SetDamping(0.99f, 0.99f);

            postPhysics += (world) =>
            {
                var mat = world * RigitBodyBone.GetMat(startTransform1).Inverted() * RigitBodyBone.GetMat(body3.WorldTransform);
                bones.SetTransform(208, mat);
            };
            */
            frameInA = Matrix.Translation(-0.0004077374f, -0.01971536f, 0);
            frameInB = Matrix.Translation(0.0004192984f, 0.03350977f, -0.000464582f);
            var generic6DofSpring1 = new Generic6DofSpring2Constraint(body2, body3, frameInA, frameInB)
            {
                //LinearUpperLimit = new Vector3(5, 0, 0),
                //LinearLowerLimit = new Vector3(-5, 0, 0),
                AngularLowerLimit = new Vector3(-20f / 180 * m_pi, -20f / 180 * m_pi, -20f / 180 * m_pi),
                AngularUpperLimit = new Vector3(20f / 180 * m_pi, 20f / 180 * m_pi, 20f / 180 * m_pi),
                //DebugDrawSize = 5
            };

            World.AddConstraint(generic6DofSpring1, true);
        }

        public virtual RigidBody LocalCreateRigidBody(float mass, Matrix startTransform, CollisionShape shape)
        {
            //rigidbody is dynamic if and only if mass is non zero, otherwise static
            bool isDynamic = (mass != 0.0f);

            Vector3 localInertia = Vector3.Zero;
            if (isDynamic)
                shape.CalculateLocalInertia(mass, out localInertia);

            //using motionstate is recommended, it provides interpolation capabilities, and only synchronizes 'active' objects
            DefaultMotionState myMotionState = new DefaultMotionState(startTransform);

            RigidBodyConstructionInfo rbInfo = new RigidBodyConstructionInfo(mass, myMotionState, shape, localInertia);
            RigidBody body = new RigidBody(rbInfo);
            rbInfo.Dispose();

            World.AddRigidBody(body);

            return body;
        }
    }
}
