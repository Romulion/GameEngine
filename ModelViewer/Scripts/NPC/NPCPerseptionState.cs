using System;
using System.Collections.Generic;
using System.Text;
using Toys;
using OpenTK.Mathematics;
using Toys.Physics;

namespace ModelViewer
{
    class NPCPerseptionState
    {
        readonly SceneNode _node;
        readonly Animator _animator;
        readonly PhysicsManager _physicsManager;
        readonly long startFrame;
        readonly BoneTransform head;
        Matrix4 headTransform;
        Matrix4 projection;
        Matrix4 lookProjection;


        bool IsSeenVal;
        bool IsSeeVal;

        public event Action OnSight;
        public event Action OnSeen;
        public event Action OnSightEnter;
        public event Action OnSeenEnter;
        public event Action OnSightExit;
        public event Action OnSeenExit;

        public NPCPerseptionState(SceneNode node)
        {
            _animator = node.GetComponent<Animator>();
            _physicsManager = node.GetComponent<PhysicsManager>();
            _node = node;
            head = _animator.BoneController.GetBone("頭");
            
            startFrame = CoreEngine.Time.FrameCount;

            //create charectes vision box
            projection = Matrix4.CreatePerspectiveFieldOfView(MathF.PI * 90f / 180f, 16f / 12f, 0.3f, 10f);
        }


        internal void Update()
        {
            //Compute look matrix every 6 frames
            if ((CoreEngine.Time.FrameCount - startFrame) % 6 == 0)
            {
                //Caclulate and cache head matrix
                headTransform = head.World2BoneInitial * head.TransformMatrix * _node.GetTransform.GlobalTransform;
                //Caclulate and cache headLook matrix
                var look = Matrix4.LookAt(headTransform.ExtractTranslation(), (new Vector4(0, 0, 1, 1) * headTransform).Xyz, (new Vector4(0, 1, 0, 1) * headTransform).Xyz);
                lookProjection = look * projection;

                //Process Sight events
                if (OnSight!= null || OnSightEnter != null || OnSightExit != null)
                {
                    if (InSight())
                    {
                        OnSight.Invoke();

                        if (!IsSeeVal)
                        {
                            OnSightEnter.Invoke();
                            IsSeeVal = true;
                        }   
                    }
                    else
                    {
                        if (IsSeeVal)
                        {
                            OnSightExit.Invoke();
                            IsSeeVal = false;
                        }
                    }
                }

                if (OnSeen != null || OnSeenEnter != null || OnSeenExit != null)
                {
                    if (IsSeen())
                    {
                        OnSeen?.Invoke();

                        if (!IsSeenVal)
                        {
                            OnSeenEnter.Invoke();
                            IsSeenVal = true;
                        }
                    }
                    else
                    {
                        if (IsSeenVal)
                        {
                            OnSeenExit.Invoke();
                            IsSeenVal = false;
                        }
                    }
                }
            }
        }

        internal bool InSight()
        {
            var point = new Vector4(CoreEngine.GetCamera.GetPos, 1) * lookProjection;
            return CheckInView(point);
        }

        internal bool IsSeen()
        {
            if (InSight())
            {
                var mvp = CoreEngine.GetCamera.GetLook * CoreEngine.GetCamera.Projection;
                
                return CheckInView(new Vector4 (headTransform.ExtractTranslation(),1) * mvp);
            }
            return false;
        }

        //Position in homogenus coordinatess
        bool CheckInView(Vector4 pos)
        {
            //Convert homogenus to cartesian coordinates
            pos /= pos.W;
            if (pos.X > 1 || pos.X < -1
                || pos.Y > 1 || pos.Y < -1
                || pos.Z > 1 || pos.Z < -1)
                return false;

            return true;
        }
    }
}
