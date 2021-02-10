using System.Runtime.InteropServices;
using BulletSharp;
using BulletSharp.Math;
using System;

namespace Toys
{
	public enum PhysPrimitiveType
	{
		Sphere,
		Box,
		Capsule,
	}


	public class RigidBodyBone
	{
        Matrix startTransform;

		public int BoneID { get; set; }
		public RigidBody Body { get; set; }
        public BoneController BoneController { get; set; }
        RigidContainer bodyContainer { get; set; }
		RigidBodyConstructionInfo rbInfo;
        public RigidBodyBone(RigidContainer rcon)
		{
            bodyContainer = rcon;
            CollisionShape shape = null;
			switch (rcon.PrimitiveType)
			{
				case PhysPrimitiveType.Box:
					shape = new BoxShape(rcon.Size.Convert());
					break;
				case PhysPrimitiveType.Capsule:
					shape = new CapsuleShape(rcon.Size.X, rcon.Size.Y);
					break;
				case PhysPrimitiveType.Sphere:
					shape = new SphereShape(rcon.Size.X);
					break;
			}

            if (rcon.Phys == PhysType.FollowBone)
                rcon.Mass = 0;
            
            bool isDynamic = (rcon.Mass != 0.0f);

            Vector3 inertia = Vector3.Zero;
            if (isDynamic)
                shape.CalculateLocalInertia(rcon.Mass, out inertia);
			startTransform = Matrix.RotationYawPitchRoll(rcon.Rotation.Y, rcon.Rotation.X, rcon.Rotation.Z) * Matrix.Translation(rcon.Position.Convert());
            rbInfo = new RigidBodyConstructionInfo(rcon.Mass, new DefaultMotionState(startTransform), shape, inertia);
			Body = new RigidBody(rbInfo);
            Body.ActivationState = ActivationState.DisableDeactivation;
            Body.Friction = rcon.Friction;
            Body.SetDamping(rcon.MassAttenuation, rcon.RotationDamping);
            Body.Restitution = rcon.Restitution;

            if (rcon.Phys == PhysType.FollowBone)
            {
                Body.SetCustomDebugColor(new Vector3(0, 1, 0));
                Body.CollisionFlags = Body.CollisionFlags | CollisionFlags.KinematicObject;
            }
            else if (rcon.Phys == PhysType.Gravity)
                Body.SetCustomDebugColor(new Vector3(1, 0, 0));
            else if (rcon.Phys == PhysType.GravityBone)
                Body.SetCustomDebugColor(new Vector3(0, 0, 1));
        }

        public void SyncBone2Body(OpenTK.Mathematics.Matrix4 world)
		{
            var mat = (BoneController.GetBone(BoneID).TransformMatrix * world).Convert();
            Body.MotionState.WorldTransform = startTransform * mat;
        }

		public void SyncBody2Bone(OpenTK.Mathematics.Matrix4 world)
		{
            var mat = startTransform.Convert().Inverted() * Body.WorldTransform.Convert();
            BoneController.GetBones[BoneID].PhysTransform = mat * world;
            BoneController.GetBones[BoneID].Phys = true;
        }

        public void SyncBody2BoneRot(OpenTK.Mathematics.Matrix4 world)
        {
            return;
            var temp = Body.WorldTransform.Convert();
            temp.M41 = startTransform.M41;
            temp.M42 = startTransform.M42;
            temp.M43 = startTransform.M43;
            var mat = startTransform.Convert().Inverted() * temp;
            BoneController.GetBones[BoneID].PhysTransform = mat * world;
            BoneController.GetBones[BoneID].Phys = true;
        }

        public void Reinstalize(OpenTK.Mathematics.Matrix4 world)
		{
            Body.WorldTransform = startTransform * world.Convert();
            Body.InterpolationWorldTransform = Body.WorldTransform;
            Body.ClearForces();
        }
    }
}
