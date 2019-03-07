using System;
using BulletSharp;
using BulletSharp.Math;
//using OpenTK;

namespace Toys
{
	public class PhysicsManager
	{
		public delegate void BoneBodySyncer(Matrix world);
		RigitBodyBone[] rigitBodies;
		Bone[] bones;
		Joint[] joints;
		public BoneBodySyncer prePhysics;
		public BoneBodySyncer postPhysics;
		Matrix world = Matrix.Identity;

		public DiscreteDynamicsWorld World { get; set; }

		public PhysicsManager(RigitContainer[] rigits, JointContainer[] jcons, Bone[] bons)
		{
			//setup world physics
			bones = bons;
			//instalize delegates
			prePhysics = (m) => { };
			postPhysics = (m) => { };

			InstalizeRigitBody(rigits);
			InstalizeJoints(jcons);
		}

		void InstalizeRigitBody(RigitContainer[] rigits)
		{
			rigitBodies = new RigitBodyBone[rigits.Length];
			for (int i = 0; i<rigits.Length; i++)
			{
				rigitBodies[i] = new RigitBodyBone(rigits[i]);
				rigitBodies[i].bone = bones[rigits[i].BoneIndex];
				World.AddRigidBody(rigitBodies[i].Body, rigits[i].GroupId, rigits[i].NonCollisionGroup);
				if (rigits[i].Phys == PhysType.FollowBone)
					prePhysics += rigitBodies[i].SyncBone2Body;
				else if (rigits[i].Phys == PhysType.GravityBone)
					prePhysics += rigitBodies[i].SyncBody2Bone;
			}
		}

		void InstalizeJoints(JointContainer[] jcons)
		{
			joints = new Joint[jcons.Length];
			for (int i = 0; i < jcons.Length; i++)
			{
				joints[i] = new Joint(jcons[i], rigitBodies);
				World.AddConstraint(joints[i].joint);
			}
		}

		public void Update()
		{
			prePhysics(world);
		}

		public void PostUpdate()
		{
			var worldInverted = world;
			worldInverted.Invert();
			postPhysics(world);
		}
	}
}
