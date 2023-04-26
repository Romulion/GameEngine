using System;
using Toys;
using OpenTK.Mathematics;
using BulletSharp;
using System.Collections.Generic;

namespace ModelViewer
{
    class NPCHeadController
    {
        public bool IsBusy { get; private set; }
        BoneTransform head;
        public RigidBody headBody;
        public SceneNode LookTarget;
        Vector2 angle;
        Vector2 targetAngle;
        float thetaDef = 0;
        float phiDef = 0;
        float thetaMin = -60 * MathF.PI / 180;
        float thetaMax = 80 * MathF.PI / 180;
        float phiMin = -70 * MathF.PI / 180;
        float phiMax = 70 * MathF.PI / 180;
        SceneNode CharNode;
        public bool IsTrackTarget = false;
        float timeScale = 1;
        double startTime;
        BezierCurveCubic headMoveCurve;
        float rotationProgress = 0;
        Vector2 startAngle;

        public NPCHeadController(SceneNode charNode)
        {
            CharNode = charNode;
            var rigged = CharNode.GetComponent<Animator>();
            if (rigged)
            {
                head = rigged.BoneController.GetBone("頭");
                head.FollowAnimation = false;
            }

            //set default position
            targetAngle = new Vector2(thetaDef, phiDef);
            angle = targetAngle;
            headMoveCurve = new BezierCurveCubic(Vector2.Zero, Vector2.One, new Vector2(0f, 1), new Vector2(0, 1) );
        }

        public void LookAt(SceneNode node)
        {
            LookTarget = node;
            HeadTrack();
        }

        internal void Update()
        {
            if (IsTrackTarget && CoreEngine.Time.FrameCount % 100 == 0)
                HeadTrack();

            if (targetAngle != angle)
            {
                /*
                var angleStep = targetAngle - angle;

                if (MathF.Abs(angleStep.X) > MathF.Abs(angSpeed.X))
                    angleStep.X = angSpeed.X;
                if (MathF.Abs(angleStep.Y) > MathF.Abs(angSpeed.Y))
                    angleStep.Y = angSpeed.Y;

                angleStep *= headMoveCurve.CalculatePoint(rotationProgress);

                rotationProgress += angleStep.LengthSquared / rotaionTotal;
                angle += angleStep;
                */
                rotationProgress += CoreEngine.FrameTimer.FrameTime;

                if (rotationProgress > 1)
                    rotationProgress = 1;


                angle = startAngle + (targetAngle - startAngle) * headMoveCurve.CalculatePoint(rotationProgress).Y;

                head.SetTransform(Quaternion.FromAxisAngle(Vector3.UnitY, -angle.X) * Quaternion.FromAxisAngle(Vector3.UnitX, -angle.Y));

            }
        }

        public void Return2Base()
        {
            targetAngle.Y = thetaDef;
            targetAngle.X = phiDef;
            CalcRotation();
        }

        //Calculate Head Rotation
        public void HeadTrack()
        {

            var headBase = head.InitialLocalTransform * head.Parent.TransformMatrix * CharNode.GetTransform.GlobalTransform;
            var headCurr = head.World2BoneInitial * head.TransformMatrix * CharNode.GetTransform.GlobalTransform;

            //var lookInit = (new Vector4(0, 0, 1, 1) * head.LocalMatrix).Xyz - head.LocalMatrix.ExtractTranslation();
            //var angleInit = Toys.MathHelper.ConvertVector2SphereAngles(lookInit);
            var lookDest = LookTarget.GetTransform.GlobalTransform.ExtractTranslation() - headCurr.ExtractTranslation();
            lookDest = (headBase * new Vector4(lookDest,1)).Xyz;
            targetAngle = Toys.MathHelper.ConvertVector2SphereAngles(lookDest);
            
            //limit rotation
            if (targetAngle.X > MathF.PI)
                targetAngle.X -= 2 * MathF.PI;
            else if (targetAngle.X < -MathF.PI)
                targetAngle.X += 2 * MathF.PI;

            if (targetAngle.Y < thetaMin)
                targetAngle.Y = thetaMin;
            else if (targetAngle.Y > thetaMax)
                targetAngle.Y = thetaMax;

            if (targetAngle.X < phiMin)
                targetAngle.X = phiMin;
            else if (targetAngle.X > phiMax)
                targetAngle.X = phiMax;

            //targetAngle -= angleInit;
            CalcRotation();
        }

        //Calculate Head Rotation Speed
        void CalcRotation()
        {
            rotationProgress = 0;
            var angleDelta = targetAngle - angle;
            startAngle = angle;
            /*
            //diagonal speed calculation
            if (angleDelta.X == 0)
            {
                angSpeed.X = 0;
                angSpeed.Y = Math.Sign(angleDelta.Y) * headSpeed;
            }
            else
            {
                angSpeed.X = (float)Math.Sqrt(Math.Pow(headSpeed, 2) * Math.Pow(angleDelta.X, 2) / (Math.Pow(angleDelta.X, 2) + Math.Pow(angleDelta.Y, 2)));
                angSpeed.Y = angSpeed.X * Math.Abs(angleDelta.Y / angleDelta.X);

                angSpeed.X = Math.Sign(angleDelta.X) * angSpeed.X;
                angSpeed.Y = Math.Sign(angleDelta.Y) * angSpeed.Y;
            }
            */
            /*
            angSpeed.Normalize();
            timeScale = (targetAngle.X - angle.X) * MathF.Sqrt(2) / headSpeed;
            startTime = CoreEngine.time.TimeFromStart;
            */
        }

    }
}
