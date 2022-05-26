using System;
using BulletSharp;
using BulletSharp.Math;
using System.Text;

namespace Toys
{
    public class PhysicsManager : ScriptingComponent
    {
        public delegate void BoneBodySyncer(OpenTK.Mathematics.Matrix4 world);
        RigidBodyBone[] rigitBodies;
        BoneController bones;
        public Joint[] joints { get; private set; }
        BoneBodySyncer prePhysics;
        BoneBodySyncer postPhysics;
        Transform worldTrans;

        DiscreteDynamicsWorld World;

        RigidContainer[] Rigits;
        JointContainer[] Jcons;

        public PhysicsManager(RigidContainer[] rigits, JointContainer[] jcons, BoneController bons)
        {
            //setup world physics
            bones = bons;
            //instalize delegates
            prePhysics = (m) => { };
            postPhysics = (m) => { };
            Rigits = rigits;
            Jcons = jcons;
            //CreateGeneric6DofSpringConstraint();
        }

        void Awake()
        {
            worldTrans = Node.GetTransform;
            World = CoreEngine.PhysEngine.World;
            
            InstalizeRigitBody(Rigits);
            InstalizeJoints(Jcons);
            ReinstalizeBodys();
            
        }

        void InstalizeRigitBody(RigidContainer[] rigits)
        {
            rigitBodies = new RigidBodyBone[rigits.Length];
            for (int i = 0; i < rigits.Length; i++)
            {
                rigitBodies[i] = new RigidBodyBone(rigits[i]);
                //skipping bone binding for no index riggs (ushort indexes only)
                if (rigits[i].BoneIndex < bones.GetBones.Length && rigits[i].BoneIndex >=0)
                {
                    rigitBodies[i].BoneID = rigits[i].BoneIndex;
                    rigitBodies[i].BoneController = bones;
                    if (rigits[i].Phys == PhysType.FollowBone)
                        prePhysics += rigitBodies[i].SyncBone2Body;

                    if (rigits[i].Phys == PhysType.Gravity)
                        postPhysics += rigitBodies[i].SyncBody2Bone;

                    if (rigits[i].Phys == PhysType.GravityBone)
                        postPhysics += rigitBodies[i].SyncBody2BoneRot;

                    rigitBodies[i].Body.UserIndex2 = i;
                }
                World.AddRigidBody(rigitBodies[i].Body, (int)Math.Pow(2, rigits[i].GroupId), rigits[i].NonCollisionGroup);

            }
        }

        void InstalizeJoints(JointContainer[] jcons)
        {
            joints = new Joint[jcons.Length];
            for (int i = 0; i < jcons.Length; i++)
            {
                joints[i] = new Joint(jcons[i], rigitBodies);
				if (joints[i].Constraint != null)
                    World.AddConstraint(joints[i].Constraint, true);
            }
        }

        void Update()
        {
            prePhysics(worldTrans.GlobalTransform);
        }

        void PreRender()
        {
            
            var worldInverted = worldTrans.GlobalTransform;
            worldInverted.Invert();
            postPhysics(worldInverted);
            
        }

		//test
		public void ReinstalizeBodys()
		{
            
            foreach (var body in rigitBodies)
            {
                body.Reinstalize(worldTrans.GlobalTransform);
            }
            
		}

        protected override void Unload()
        {
            if (!IsInstalized)
                return;

            foreach (var joint in joints)
            {
                World.RemoveConstraint(joint.Constraint);
                joint.Constraint.Dispose();
            }

            foreach (var rigid in rigitBodies)
            {
                World.RemoveCollisionObject(rigid.Body);
                rigid.Body.MotionState.Dispose();
                rigid.Body.RemoveCustomDebugColor();
                rigid.Body.Dispose();
            }
            base.Unload();
        }

        internal override Component Clone()
        {
            return new PhysicsManager(Rigits, Jcons, bones);
        }
    }
}
