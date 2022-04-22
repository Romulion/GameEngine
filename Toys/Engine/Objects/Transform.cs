using System;
using OpenTK.Mathematics;

namespace Toys
{

    public class Transform : Component
    {
        private Matrix4 localT;
        Quaternion rotationQuaternion;
		Vector3 rotation;
		Vector3 position;
        Vector3 scale;
        const float ratio = (float)(Math.PI / 180);

        /// <summary>
        /// Get Matrix in World Coordinates
        /// </summary>
        public Matrix4 GlobalTransform
        {
            get; private set;
        } 

        internal Transform()
        {
            localT = Matrix4.Identity;
            GlobalTransform = Matrix4.Identity;
            rotationQuaternion = Quaternion.Identity;
            rotation = Vector3.Zero;
            position = Vector3.Zero;
            scale = Vector3.One;
        }
    
        /// <summary>
        /// Local Transformation Matrix (Model space)
        /// </summary>
        public Matrix4 GetTransform
        {
            get { return localT; }
        }

        /// <summary>
        /// Position of Node Relative to Parent
        /// </summary>
        public Vector3 Position
        {
            get
            {
                return position;
            }

            set
            {
                position = value;
                RecalculateLocal();
            }
        }

        /// <summary>
        /// Scale of Node Object
        /// </summary>
        public Vector3 Scale
        {
            get
            {
                return scale;
            }

            set
            {
                scale = value;
                RecalculateLocal();
            }
        }


        /// <summary>
        /// Euler Angle Rotation; Order XYZ; Degrees
        /// </summary>
        public Vector3 Rotation
        {
            
            get
            {
                Quaternion.ToEulerAngles(rotationQuaternion, out rotation);
                return rotation / ratio;
            }

            set
            {
                rotation = value * ratio;
                rotationQuaternion = Quaternion.FromEulerAngles(rotation);
                RecalculateLocal();
            }
        }

        /// <summary>
        /// Rotation Quaternion
        /// </summary>
        public Quaternion RotationQuaternion
        {
            get
            {
                return rotationQuaternion;
            }

            set
            {
                rotationQuaternion = value;
                rotation = rotationQuaternion.ToEulerAngles();
                RecalculateLocal();

               // Console.WriteLine(localT);
            }
        }

        public Vector3 Forward
        {
            get
            {
                var res = -Vector3.UnitZ;
                res = (new Vector4(res) * Node.GetTransform.GlobalTransform).Xyz;
                return res.Normalized();
            }
        }

        public Vector3 Backward
        {
            get
            {
                var res = Vector3.UnitZ;

                res = (new Vector4(res) * Node.GetTransform.GlobalTransform).Xyz;
                return res.Normalized();
            }
        }

        public Vector3 Up
        {
            get
            {
                var res = (Vector4.UnitY * Node.GetTransform.GlobalTransform).Xyz;

                return res.Normalized();
            }
        }


        public Vector3 Down
        {
            get
            {
                var res = -Vector3.UnitY;

                res = (new Vector4(res) * Node.GetTransform.GlobalTransform).Xyz;
                return res.Normalized();
            }
        }

        public Vector3 Right
        {
            get
            {
                var res = Vector3.UnitX;
                res = (new Vector4(res) * Node.GetTransform.GlobalTransform).Xyz;
                
                return res.Normalized();
            }
        }

        public Vector3 Left
        {
            get
            {
                var res = -Vector3.UnitX;
                res = (new Vector4(res) * Node.GetTransform.GlobalTransform).Xyz;
                return res.Normalized();
            }
        }

        public Vector3 GlobalPosition
        {
            get
            {
                return GlobalTransform.ExtractTranslation();
            }
        }

        public Quaternion GlobalRotaion
        {
            get
            {
                return GlobalTransform.ExtractRotation();
            }
        }

        public void LookAt(Vector3 target)
        {
            var pos = GlobalPosition;
            target.Y = pos.Y;
            var look = Matrix4.LookAt(pos, target, Up);
            if (float.IsNaN(look.M11))
                return;
            localT = (Node.Parent.GetTransform.GlobalTransform * look).Inverted() * Matrix4.CreateFromAxisAngle(Vector3.UnitY, MathF.PI);
            UpdateGlobalTransform();
        }

        internal void UpdateGlobalTransform()
        {
            if (Node.Parent != null)
            {
                GlobalTransform = localT * Node.Parent.GetTransform.GlobalTransform;
            }
            else
                GlobalTransform = localT;
        }

        private void RecalculateLocal()
        {
            localT = Matrix4.CreateScale(scale) * Matrix4.CreateFromQuaternion(rotationQuaternion) * Matrix4.CreateTranslation(position);
            UpdateGlobalTransform();
        }

        internal override void AddComponent(SceneNode nod)
        {
            Node = nod;
        }

        internal override void RemoveComponent()
        {
            Node = null;
        }
        internal override void Unload()
        {
        }
    }
}
