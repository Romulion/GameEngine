using System;
using System.Collections.Generic;
using System.Linq;

namespace Toys
{
    class MorphMaterial : Morph
    {
        float degree = 0f;
        public List<MaterialMorpher> MaterialMorphers;

        public MorphMaterial(string name, string nameEng, int count)
        {
            Name = name;
            NameEng = nameEng;
            Type = MorphType.Material;
            MaterialMorphers = new List<MaterialMorpher>();
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

        void PerformMorph(float value)
        {
            foreach (var materialMorpher in MaterialMorphers)
                materialMorpher.Perform(value);
        }
    }
}
