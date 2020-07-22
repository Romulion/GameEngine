using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BulletSharp;
using BulletSharp.Math;
//using OpenTK;

namespace Toys
{
    public abstract class CharacterControllBase : ScriptingComponent
    {
        DiscreteDynamicsWorld world;
        KinematicCharacterController _charController;
        protected PairCachingGhostObject _ghostObject;
        public float WalkSpeed { get; set; }
        public float RunSpeed { get; set; }
        protected void Awake()
        {
            WalkSpeed = 1.1f * 4.0f;
            RunSpeed = WalkSpeed * 2;
            world = CoreEngine.pEngine.World;
            CreatePlayerBox();
        }

        private void CreatePlayerBox()
        {
            const float stepHeight = 0.35f;
            CapsuleShape shape = new CapsuleShape(1, 1.7f);
            _ghostObject = new PairCachingGhostObject()
            {
                CollisionShape = shape,
                CollisionFlags = CollisionFlags.CharacterObject,
                WorldTransform = Matrix.Translation(Node.GetTransform.Position.Convert() + Vector3.UnitY)
            };
            world.AddCollisionObject(_ghostObject, CollisionFilterGroups.CharacterFilter, CollisionFilterGroups.StaticFilter | CollisionFilterGroups.DefaultFilter);
            _charController = new KinematicCharacterController(_ghostObject, shape, stepHeight);
            world.AddAction(_charController);
        }

        protected void Walk(float frameDelta)
        {
            if (_charController.OnGround)
            {
                var mat = Node.GetTransform.GlobalTransform;
                var direction = new Vector3(mat.M31, 0, mat.M33);
                direction.Normalize();
                _charController.SetWalkDirection(direction * frameDelta * WalkSpeed);
            }
        }

        protected void Jump()
        {
            _charController.Jump();
        }

        protected void Stop()
        {
            if (_charController.OnGround)
                _charController.SetWalkDirection(Vector3.Zero);
        }

        protected void Update()
        {
            //update node position
            var transform = _ghostObject.WorldTransform;
            Node.GetTransform.Position = new OpenTK.Vector3(transform.M41, transform.M42, transform.M43);
        }

        protected void Teleport(OpenTK.Vector3 position)
        {
            var pos = position.Convert();
            _charController.Warp(ref pos);
        }
    }
}
