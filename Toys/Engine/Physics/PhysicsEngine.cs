using System;
using BulletSharp;
using BulletSharp.Math;

namespace Toys
{
	public class PhysicsEngine
	{
		CollisionDispatcher dispatcher;
		DbvtBroadphase broadphase;
		CollisionConfiguration collisionConf;
		public DiscreteDynamicsWorld World { get; private set; }

		public PhysicsEngine()
		{
			collisionConf = new DefaultCollisionConfiguration();
			dispatcher = new CollisionDispatcher(collisionConf);
			broadphase = new DbvtBroadphase();
			World = new DiscreteDynamicsWorld(dispatcher, broadphase, null, collisionConf);
			World.Gravity = new Vector3(0, -10, 0);
			CreateFloor();
		}


		public void Update(float elapsedTime)
		{
			World.StepSimulation(elapsedTime);
		}


		void CreateFloor()
		{
			const float staticMass = 0;
			RigidBody body;
			CollisionShape shape = new BoxShape(100, 10, 100);
			Matrix groundTransform = Matrix.Translation(0, -10, 0);
			using (var rbInfo = new RigidBodyConstructionInfo(staticMass, null, shape)
			{
				StartWorldTransform = groundTransform
			})
			{
				body = new RigidBody(rbInfo);
			}

			World.AddRigidBody(body);
		}
	}
}
