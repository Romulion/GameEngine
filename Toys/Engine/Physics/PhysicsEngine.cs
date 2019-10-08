using System;
using BulletSharp;
using BulletSharp.Math;
using System.Collections.Generic;

namespace Toys
{
	public class PhysicsEngine : IDisposable
	{
		CollisionDispatcher dispatcher;
		DbvtBroadphase broadphase;
		CollisionConfiguration collisionConf;
        ConstraintSolverPoolMultiThreaded Solver;

        private int MaxThreadCount = 4;
        private int currentScheduler;
        private List<TaskScheduler> schedulers = new List<TaskScheduler>();

        public DiscreteDynamicsWorld World { get; private set; }

		public PhysicsEngine()
		{
            //CreateSchedulers();
            //NextTaskScheduler();
            using (var collisionConfigurationInfo = new DefaultCollisionConstructionInfo
            {
                DefaultMaxPersistentManifoldPoolSize = 800,
                DefaultMaxCollisionAlgorithmPoolSize = 800
            })
            {
                collisionConf = new DefaultCollisionConfiguration(collisionConfigurationInfo);
            };
            dispatcher = new CollisionDispatcher(collisionConf);   
            broadphase = new DbvtBroadphase();
            var solver = new SequentialImpulseConstraintSolver();
            //var Solver = new NncgConstraintSolver();
            //DiscreteDynamicsWorldMultiThreaded(dispatcher, broadphase, Solver, collisionConf);
            World = new DiscreteDynamicsWorld(dispatcher, broadphase, solver, collisionConf);

            //dispatcher = new CollisionDispatcherMultiThreaded(collisionConf);
            //Solver = new ConstraintSolverPoolMultiThreaded(MaxThreadCount);
            //World = new DiscreteDynamicsWorldMultiThreaded(dispatcher, broadphase, Solver, null, collisionConf);
            //World.SolverInfo.SolverMode = SolverModes.Simd | SolverModes.UseWarmStarting;

            //World = new DiscreteDynamicsWorldMultiThreaded(dispatcher, broadphase, cspm, csM, collisionConf);
            //World.Gravity = new Vector3(0, 0f, 0);
            CreateFloor();
            //World.LatencyMotionStateInterpolation = false;

            World.DebugDrawer = new PhysicsDebugDraw(World);
        }


		public void Update(float elapsedTime)
		{
            World.StepSimulation(elapsedTime,100);
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

        public void NextTaskScheduler()
        {
            currentScheduler++;
            if (currentScheduler >= schedulers.Count)
            {
                currentScheduler = 0;
            }
            TaskScheduler scheduler = schedulers[currentScheduler];
            scheduler.NumThreads = scheduler.MaxNumThreads;
            Threads.TaskScheduler = scheduler;
        }

        private void CreateSchedulers()
        {
            AddScheduler(Threads.GetSequentialTaskScheduler());
            AddScheduler(Threads.GetOpenMPTaskScheduler());
            AddScheduler(Threads.GetTbbTaskScheduler());
            AddScheduler(Threads.GetPplTaskScheduler());
        }

        private void AddScheduler(TaskScheduler scheduler)
        {
            if (scheduler != null)
            {
                schedulers.Add(scheduler);
            }
        }

        public void Dispose()
        {
            //remove/dispose constraints
            int i;
            for (i = World.NumConstraints - 1; i >= 0; i--)
            {
                TypedConstraint constraint = World.GetConstraint(i);
                World.RemoveConstraint(constraint);
                constraint.Dispose();
            }

            //remove the rigidbodies from the dynamics world and delete them
            for (i = World.NumCollisionObjects - 1; i >= 0; i--)
            {
                CollisionObject obj = World.CollisionObjectArray[i];
                RigidBody body = obj as RigidBody;
                if (body != null && body.MotionState != null)
                {
                    body.MotionState.Dispose();
                }
                World.RemoveCollisionObject(obj);
                obj.Dispose();
            }

            //delete collision shapes
            foreach (var shape in World.CollisionObjectArray)
                shape.Dispose();

            World.Dispose();
            broadphase.Dispose();
            if (dispatcher != null)
            {
                dispatcher.Dispose();
            }
            collisionConf.Dispose();
        }
	}
}
