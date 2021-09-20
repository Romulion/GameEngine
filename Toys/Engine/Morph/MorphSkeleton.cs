using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Mathematics;

namespace Toys
{
    public class MorphSkeleton : Morph
    {
        BoneController boneController;
        Tuple<int,Vector3,Quaternion>[] bones;
        int offset = 0;

        public MorphSkeleton(string name, string nameEng, int count, BoneController bc)
        {
            Name = name;
            NameEng = nameEng;
            boneController = bc;
            bones = new Tuple<int, Vector3, Quaternion>[count];
        }

        public void AddBone(int index, Vector3 pos, Quaternion rot)
        {
            bones[offset] = new Tuple<int, Vector3, Quaternion>(index, pos, rot);
            offset++;
        }

        public override float MorphDegree
        {
            get
            {
                return degree;
            }
            set
            {
                PerformMorph(value);
                degree = value;
            }
        }

        private void PerformMorph(float degree)
        {
            if (boneController == null)
                return;

            foreach (var bone in bones)
            {
                var pos = bone.Item2 * degree;
                var rot = Quaternion.Slerp(Quaternion.Identity, bone.Item3, degree);
                boneController.GetBone(bone.Item1).SetTransform(rot, pos);
            }
        }
    }

}
