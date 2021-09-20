using System;
using System.Collections.Generic;
using System.Text;

namespace Toys
{
    public class MorphGroup : Morph
    {
        Morph[] morphList;
        Tuple<int,float>[] morphIds;
        int offset = 0;

        public MorphGroup(string name, string nameEng, int count, Morph[] morphs)
        {
            Name = name;
            NameEng = nameEng;
            morphList = morphs;
            morphIds = new Tuple<int, float>[count];
        }

        public void AddMorph(int index, float impact)
        {
            morphIds[offset] = new Tuple<int, float>(index, impact);
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
            if (morphList == null)
                return;

            foreach (var morph in morphIds)
                morphList[morph.Item1].MorphDegree = morph.Item2 * degree;
        }
    }
}
