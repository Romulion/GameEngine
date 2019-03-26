using System;
using BulletSharp;
using BulletSharp.Math;
using System.Text;

namespace Toys
{
    public class PhysicsManager
    {
        public delegate void BoneBodySyncer(OpenTK.Matrix4 world);
        RigidBodyBone[] rigitBodies;
        BoneController bones;
        Joint[] joints;
        public BoneBodySyncer prePhysics;
        public BoneBodySyncer postPhysics;
        Transformation worldTrans;

		public DiscreteDynamicsWorld World { get; private set; }

        public PhysicsManager(RigidContainer[] rigits, JointContainer[] jcons, BoneController bons, Transformation trans)
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

        void InstalizeRigitBody(RigidContainer[] rigits)
        {
            rigitBodies = new RigidBodyBone[rigits.Length];
            for (int i = 0; i < rigits.Length; i++)
            {

                //if (i != 2 && (i < 103 || i > 104))
                //    continue;

				rigitBodies[i] = new RigidBodyBone(rigits[i]);

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
				if (joints[i].joint != null)
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
		public void ReinstalizeBodys()
		{

			foreach (var body in rigitBodies)
				body.Reinstalize(worldTrans.globalTransform);
			
		}
    }
}
