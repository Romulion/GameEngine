using System;
using BulletSharp;
using BulletSharp.Math;
using System.Collections.Generic;

namespace Toys
{
    //Flags for filltering collisions
    // 16 - 31 bits reserved for cloth collision
    [Flags]
    public enum CollisionFilleters
    {
        All = -1,
        None = 0,
        Default = 1,
        Static = 2,
        Kinematic = 4,
        Debris = 8,
        Sensor = 16,
        Character = 32,
        Player = 64,
        Look = 128,
        Projectile  = 256,
    }
	public class PhysicsEngine : IDisposable
	{
        internal Action Body2Scene;
        internal Action Scene2Body;

        CollisionDispatcher dispatcher;
		DbvtBroadphase broadphase;
		CollisionConfiguration collisionConf;
        //ConstraintSolverPoolMultiThreaded Solver;

        //private int MaxThreadCount = 4;
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
           // CreateFloor();
            //World.LatencyMotionStateInterpolation = false;

            //for character controllers
            broadphase.OverlappingPairCache.SetInternalGhostPairCallback(new GhostPairCallback());

            World.DebugDrawer = new PhysicsDebugDraw(World);
        }


		public void Update(float elapsedTime)
		{
            World.StepSimulation(elapsedTime,10);
        }


		void CreateFloor()
		{
			const float staticMass = 0;
			RigidBody body;
			CollisionShape shape = new BoxShape(50, 0.5f, 50);
			Matrix groundTransform = Matrix.Translation(0, -0.5f, 0);
			using (var rbInfo = new RigidBodyConstructionInfo(staticMass, null, shape)
			{
				StartWorldTransform = groundTransform,
			})
			{
				body = new RigidBody(rbInfo);
			}
			World.AddRigidBody(body, CollisionFilterGroups.StaticFilter, CollisionFilterGroups.AllFilter);
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

        public void SetScene(OpenTK.Mathematics.Vector3[] mesh, int[] indexes, OpenTK.Mathematics.Matrix4 startTransform)
        {
            //convert openTK to BulentSharp
            var meshP = new Vector3[mesh.Length];
            for (int i = 0; i < mesh.Length; i++)
            {
                meshP[i] = mesh[i].Convert();
            }

            TriangleIndexVertexArray triangles = new TriangleIndexVertexArray(indexes, meshP);
            CollisionShape shape = new BvhTriangleMeshShape(triangles,true);
            shape.CalculateLocalInertia(0);
            RigidBodyConstructionInfo rbInfo = new RigidBodyConstructionInfo(0, new DefaultMotionState(startTransform.Convert()), shape, Vector3.Zero);
            RigidBody Body = new RigidBody(rbInfo);
            Body.CollisionFlags = CollisionFlags.StaticObject;
            Body.UserObject = "Ground";

            World.AddRigidBody(Body, CollisionFilterGroups.StaticFilter, CollisionFilterGroups.CharacterFilter);
            /*
            const float staticMass = 0;
            RigidBody body;
            CollisionShape shape = new TriangleMesh()
            Matrix groundTransform = Matrix.Translation(0, -0.5f, 0);
            using (var rbInfo = new RigidBodyConstructionInfo(staticMass, null, shape)
            {
                StartWorldTransform = groundTransform,
            })
            {
                body = new RigidBody(rbInfo);
            }
            World.AddRigidBody(body, CollisionFilterGroups.StaticFilter, CollisionFilterGroups.AllFilter);
            */
        }

        public void SetGravity(OpenTK.Mathematics.Vector3 grav)
        {
            World.Gravity = new Vector3(grav.X, grav.Y, grav.Z);
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
