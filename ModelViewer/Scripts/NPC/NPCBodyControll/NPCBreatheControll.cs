using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Mathematics;
using Toys;

namespace ModelViewer
{
    class NPCBreatheControll
    {
        public bool IsBreathing;
        //semi breathe time
        public float BreatheTime = 2.3f;
        BoneController boneController;
        BoneTransform[] bones;
        string[] boneNames;
        Quaternion[] minMaxValues;
        bool Inhale = true;
        float time;

        public NPCBreatheControll(Animator a)
        {
            
            boneController = a.BoneController;
            SetupValues();
        }

        void SetupValues()
        {
            boneNames = new string[] { "上半身", "上半身2", "首", "右肩", "左肩" };
            minMaxValues = new Quaternion[]
            {
                new Quaternion(-0.005f, 0, 0, 1),
                new Quaternion(0.006f, 0, 0, 1),
                new Quaternion(0.015f, 0, 0, 1),
                new Quaternion(-0.030f, 0.006f, -0.001f, 1),
                new Quaternion(0.030f, -0.006f, -0.001f, 1),
            };
            bones = new BoneTransform[5];
            for(int i = 0; i < 5; i++)
                minMaxValues[i].Normalize();

            for (int i = 0; i < boneNames.Length; i++)
                bones[i] = boneController.GetBone(boneNames[i]);

        }

        internal void Update()
        {
            if (IsBreathing)
            {
                
                time += CoreEngine.FrameTimer.FrameTime;
                
                if (time > BreatheTime)
                {
                    Inhale = !Inhale;
                    time %= BreatheTime;
                }
                var progress = time / BreatheTime;

                
                for (int i = 0; i < 5; i++)
                {
                    if (bones[i] == null)
                        continue;

                    if (Inhale)
                        bones[i].SetTransform(Quaternion.Slerp(Quaternion.Identity, minMaxValues[i], progress));
                    else
                        bones[i].SetTransform(Quaternion.Slerp(minMaxValues[i], Quaternion.Identity, progress));
                }
            }
        }
    }
}
