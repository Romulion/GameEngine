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
            //dispatcher = new CollisionDispatcherMultiThreaded(collisionConf);
            broadphase = new DbvtBroadphase();
            var Solver = new SequentialImpulseConstraintSolver();
            World = new DiscreteDynamicsWorld(dispatcher, broadphase, Solver, collisionConf);
			World.Gravity = new Vector3(0, -10f, 0);
			CreateFloor();
		}


		public void Update(float elapsedTime)
		{
            World.StepSimulation(elapsedTime/1000);
		}


		void CreateFloor()
		{
			const float staticMass = 0;
			RigidBody body;
			CollisionShape shape = new BoxShape(2, 0.5f, 2);
			Matrix groundTransform = Matrix.Translation(0, -0.5f, 0);
			using (var rbInfo = new RigidBodyConstructionInfo(staticMass, null, shape)
			{
				StartWorldTransform = groundTransform
			})
			{
				body = new RigidBody(rbInfo);
			}

			World.AddRigidBody(body,1, short.MaxValue);
		}
	}
}
