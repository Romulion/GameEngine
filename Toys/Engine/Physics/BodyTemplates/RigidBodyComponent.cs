using System;
using System.Collections.Generic;
using System.Text;
using BulletSharp;
using BulletSharp.Math;

namespace Toys
{
    public class RigidBodyComponent : Component
    {
        public RigidBody body;
        protected CollisionShape shape;
        protected float mass = 0;
        protected RigidBodyConstructionInfo rbInfo;
        public OpenTK.Mathematics.Vector3 Center { get; set; }

        CollisionCallback callBack = (c) => { };
        int callBackCount = 0;
        public event CollisionCallback OnHit
        {
            add
            {
                if (callBackCount == 0)
                {
                    body.CollisionFlags |= CollisionFlags.CustomMaterialCallback;
                    ManifoldPoint.ContactAdded += CollisionCallbackParser;
                }

                callBack += value;
                callBackCount++;
            }
            remove
            {
                callBack -= value;
                callBackCount--;
                if (callBackCount == 0)
                {
                    body.CollisionFlags -= body.CollisionFlags & CollisionFlags.CustomMaterialCallback;
                    ManifoldPoint.ContactAdded -= CollisionCallbackParser;
                }
            }
        }



        public RigidBodyComponent()
        {
            GroupFlags = -1;
            CollisionGroupFlags = -1;
            Center = OpenTK.Mathematics.Vector3.Zero;
        }
        public int GroupFlags { get; private set; }
        public int CollisionGroupFlags { get; private set; }

        public float Mass
        {
            get { return mass; }
            set
            {
                mass = value;
                RecalculateInertia();
            }
        }


        public bool IsKinematic
        {
            get
            {
                return (body.CollisionFlags & CollisionFlags.KinematicObject) > 0;
            }
            set
            {
                if (value)
                    body.CollisionFlags |= CollisionFlags.KinematicObject;
                else
                    body.CollisionFlags -= body.CollisionFlags & CollisionFlags.KinematicObject;
            }
        }

        protected void RecalculateInertia(bool set = false)
        {
            if (Node)
                CoreEngine.pEngine.World.RemoveCollisionObject(body);
            if (set)
            {
                body.MotionState.Dispose();
                body.Dispose();
            }

            Vector3 inertia = Vector3.Zero;
            shape.CalculateLocalInertia(mass, out inertia);
            if (Node)
                rbInfo = new RigidBodyConstructionInfo(mass, new DefaultMotionState(Node.GetTransform.GlobalTransform.Convert()), shape, inertia);
            else
                rbInfo = new RigidBodyConstructionInfo(mass, new DefaultMotionState(Matrix.Identity), shape, inertia);
            body = new RigidBody(rbInfo);

            if (Node)
                CoreEngine.pEngine.World.AddRigidBody(body,  GroupFlags, CollisionGroupFlags);
        }



        private void CollisionCallbackParser(ManifoldPoint cp, CollisionObjectWrapper colObj0Wrap, int partId0, int index0, CollisionObjectWrapper colObj1Wrap, int partId1, int index1)
        {
            if (colObj0Wrap.CollisionObject == body)
            {
                CollisionCallback(cp, colObj1Wrap);
            }
            else if (colObj1Wrap.CollisionObject == body)
            {
                CollisionCallback(cp, colObj0Wrap);
            }

            void CollisionCallback(ManifoldPoint cp, CollisionObjectWrapper colObjWrap)
            {
                var data = new CollisionData(colObjWrap.CollisionObject, cp.AppliedImpulse);
                callBack?.Invoke(data);
            }
        }




        private void UpdateBody()
        {
            body.WorldTransform = Node.GetTransform.GlobalTransform.Convert();
        }
        private void UpdateNode()
        {

            var mat = Node.Parent.GetTransform.GlobalTransform.Inverted() * body.WorldTransform.Convert();
            Node.GetTransform.Position = mat.ExtractTranslation() ;
            Node.GetTransform.RotationQuaternion = mat.ExtractRotation();
        }

        public void SetFlags(int groupFlags, int collisionGroupFlags)
        {
            CoreEngine.pEngine.World.RemoveRigidBody(body);
            GroupFlags = groupFlags;
            CollisionGroupFlags = collisionGroupFlags;
            CoreEngine.pEngine.World.AddRigidBody(body, GroupFlags, CollisionGroupFlags);
        }

        internal override void AddComponent(SceneNode nod)
        {
            Node = nod;
            CoreEngine.pEngine.World.AddRigidBody(body, GroupFlags, CollisionGroupFlags);
            CoreEngine.pEngine.Scene2Body += UpdateBody;
            //CoreEngine.pEngine.Body2Scene += UpdateNode;
        }

        internal override void RemoveComponent()
        {
            Node = null;
            
            CoreEngine.pEngine.Scene2Body -= UpdateBody;
            //CoreEngine.pEngine.Body2Scene -= UpdateNode;
            
            CoreEngine.pEngine.World.RemoveCollisionObject(body);
            body.MotionState.Dispose();
            body.Dispose();
        }

        internal override void Unload()
        {
            body.Dispose();
        }
    }
}
