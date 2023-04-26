using System;
using System.Collections.Generic;
using System.Linq;
using Toys;

namespace ModelViewer
{
    class ClothPieceWrapper
    {
        public bool IsOn { get; private set; }
        public bool IsShifted { get; private set; }
        readonly ClothingPiece cloth;
        readonly Material[] materials;
        readonly Morph morph;

        public ClothPieceWrapper(MeshDrawer mesh, ClothingPiece clothp)
        {
            cloth = clothp;
            IsOn = true;
            var mats = new List<Material>(cloth.mateials.Count);
            for (int i = 0; i < cloth.mateials.Count; i++)
            {
                Material mat = null;
                try
                {
                    mat = mesh.Materials.First((m) => m.Name == cloth.mateials.ElementAt(i).Key);
                }
                catch (InvalidOperationException)
                {
                    Logger.Warning(String.Format("Cloth material not found {0} for {1}", cloth.mateials.ElementAt(i).Key, clothp.Slot));
                    continue;
                }

                mats.Add(mat);
            }
            materials = mats.ToArray();

            if (cloth.Morph != null)
            {
                Morph mor = null;
                try
                {
                    mor = mesh.Morphes.First((m) => m.Name == cloth.Morph);
                }
                catch (InvalidOperationException)
                {
                    Logger.Warning(String.Format("Cloth morph not found {0} for ", cloth.Morph, clothp.Slot));
                }

                morph = mor;
            }
        }

        public bool HasMorph
        {
            get
            {
                return morph != null;
            }
        }

        public void Shift()
        {
            if (HasMorph)
                morph.MorphDegree = 1;
            IsShifted = true;
        }

        public void UnShift()
        {
            if (HasMorph)
                morph.MorphDegree = 0;
            IsShifted = false;
        }

        public void Remove()
        {
            for(int i = 0; i < materials.Length; i++)
            {
                materials[i].RenderDirrectives.IsRendered = cloth.mateials.ElementAt(i).Value;
            }

            IsOn = false;
        }

        public void PutOn()
        {
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i].RenderDirrectives.IsRendered = !cloth.mateials.ElementAt(i).Value;
            }

            IsOn = true;
        }
    }
}
