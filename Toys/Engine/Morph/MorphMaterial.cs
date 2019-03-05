using System;
using System.Collections.Generic;
using System.Linq;

namespace Toys
{
    class MorphMaterial : Morph
    {
        float degree = 0f;
        public List<MaterialMorpher> matMorpher;
        int curr = 0;

        public MorphMaterial(string Name, string NameEng, int count)
        {
            base.Name = Name;
            base.NameEng = NameEng;
            type = MorphType.Material;
            matMorpher = new List<MaterialMorpher>();
        }

        public override float morphDegree
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

        void PerformMorph(float value)
        {
            foreach (var mat in matMorpher)
                mat.Perform(value);
        }
    }
}
