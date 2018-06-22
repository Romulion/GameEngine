using System;
using BulletSharp;
using BulletSharp.Math;
//using OpenTK;

namespace Toys
{
	public enum PhysPrimitiveType
	{
		Sphere,
		Box,
		Capsule,
	}


	public class PhysicsManager
	{
		RigitContainer[] rigitBodies;
		CollisionShape[] collisionShapes;

		CollisionDispatcher dispatcher;
		DbvtBroadphase broadphase;
		CollisionConfiguration collisionConf;
		public DiscreteDynamicsWorld World { get; set; }

		public PhysicsManager(RigitContainer[] rigits)
		{
			//setup world physics
			collisionConf = new DefaultCollisionConfiguration();
			dispatcher = new CollisionDispatcher(collisionConf);
			broadphase = new DbvtBroadphase();
			World = new DiscreteDynamicsWorld(dispatcher, broadphase, null, collisionConf);
			World.Gravity = new Vector3(0, -10, 0);

			rigitBodies = rigits;
			collisionShapes = new CollisionShape[rigits.Length];
			for (int i = 0; i < rigits.Length; i++)
			{
				var rigit = rigits[i];
				CollisionShape shape = null;
				switch (rigit.primitive)
				{
					case PhysPrimitiveType.Box:
						shape = new BoxShape(GetVec3(rigit.Size));
						break;
					case PhysPrimitiveType.Capsule:
						shape = new CapsuleShape(rigit.Size.X, rigit.Size.Y);
						break;
					case PhysPrimitiveType.Sphere:
						shape = new SphereShape(rigit.Size.X);
						break;
				}
				collisionShapes[i] = shape;

				Vector3 inertia = shape.CalculateLocalInertia(rigit.Mass);

				var rbInfo = new RigidBodyConstructionInfo(rigit.Mass, null, shape, inertia);
				rbInfo.MotionState = new DefaultMotionState(Matrix.Translation(GetVec3(rigit.Position)));
				RigidBody body = new RigidBody(rbInfo);
				body.Friction = rigit.Friction;
				body.SetDamping(0f, rigit.RotationDamping);
                World.AddRigidBody(body);
				rbInfo.Dispose();
				//HingeConstraint joint = new HingeConstraint(
			}
		}


		public virtual void Update(float elapsedTime)
		{
			World.StepSimulation(elapsedTime);
		}

		private Vector3 GetVec3(OpenTK.Vector3 vec3)
		{
			return new Vector3(vec3.X, vec3.Y, vec3.Z);
		}
	}
}
