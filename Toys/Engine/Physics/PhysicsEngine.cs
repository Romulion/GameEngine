using System;
using BulletSharp;
using BulletSharp.Math;

namespace Toys
{
	public class PhysicsEngine : IDisposable
	{
		CollisionDispatcher dispatcher;
		DbvtBroadphase broadphase;
		CollisionConfiguration collisionConf;
		public DiscreteDynamicsWorld World { get; private set; }

		public PhysicsEngine()
		{
            using (var collisionConfigurationInfo = new DefaultCollisionConstructionInfo
            {
                DefaultMaxPersistentManifoldPoolSize = 80000,
                DefaultMaxCollisionAlgorithmPoolSize = 80000
            })
            {
                collisionConf = new DefaultCollisionConfiguration(collisionConfigurationInfo);
            };
            dispatcher = new CollisionDispatcher(collisionConf);
            broadphase = new DbvtBroadphase();
            var Solver = new SequentialImpulseConstraintSolver();
            //DiscreteDynamicsWorldMultiThreaded(dispatcher, broadphase, Solver, collisionConf);
            World = new DiscreteDynamicsWorld(dispatcher, broadphase, Solver, collisionConf);

            //World = new DiscreteDynamicsWorldMultiThreaded(dispatcher, broadphase, cspm, csM, collisionConf);
            World.Gravity = new Vector3(0, -9.8f, 0);
			CreateFloor();
            //World.LatencyMotionStateInterpolation = false;
        }


		public void Update(float elapsedTime)
		{
            World.StepSimulation(elapsedTime,4);
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

        public void Dispose()
        {
            broadphase.Dispose();
            dispatcher.Dispose();
            World.Dispose();
            collisionConf.Dispose();
        }
	}
}
